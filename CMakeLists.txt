cmake_minimum_required(VERSION 3.11)
project("Taiwu_Mods" CSharp)

SET(DLLPATH "The Scroll Of Taiwu Alpha V1.0_Data\\Managed\\")
set(CMAKE_CSharp_FLAGS "/langversion:6")
include(CSharpUtilities)
include(CMakeModules/JSONParser.cmake)

#遍历Mod 目录
MACRO(SUBDIRLIST result curdir)
  #message(STATUS "search sub dir in:" ${curdir})
  FILE(GLOB children RELATIVE ${curdir} ${curdir}/*)
  SET(dirlist "")
  FOREACH(child ${children})
    IF(IS_DIRECTORY ${curdir}/${child} AND  (NOT ${child} STREQUAL "build") AND (NOT ${child} STREQUAL ".git") AND (NOT ${child} STREQUAL "CMakeModules"))
	  #message(STATUS "Found Mod: " ${child})
      LIST(APPEND dirlist ${child})
    ENDIF()
  ENDFOREACH()
  SET(${result} ${dirlist})
ENDMACRO()


#为每个目录生成Mod
MACRO(CREATEMOD moddir)

	# 解析mod目录下 Info.json 文件， 生成对应 Properties/AssemblyInfo.cs ,添加mod依赖
	#读取文件
	file(STRINGS ${moddir}/Info.json infojson)
	#初始化变量
	set(modinfo "")
	set(Id "")
	set(DisplayName "")
	set(Author "")
	set(Version "")
	set(Requirements "")
	set(modinfo.Requirements "")
	#解析json
	sbeParseJson(modinfo infojson)

	# 调试代码：列出json值
	#foreach(var ${modinfo})
	#	message("vals : ${var} = ${${var}}")
	#endforeach()
	#取json值
	set(Id ${modinfo.Id})
	set(DisplayName ${modinfo.DisplayName})
	set(Author ${modinfo.Author})
	set(Version ${modinfo.Version})
	set(Requirements ${modinfo.Requirements})

	message(STATUS "Found Mod: ${Id}  ${DisplayName} Author ${Author}  Version ${Version} ${CMAKE_CURRENT_BINARY_DIR} ")
	
	SET(FILELIST "")
	#遍历所有.cs文件
	FILE(GLOB_RECURSE FILES ${moddir}/*.cs)
	FOREACH(csfile ${FILES})
		  LIST(APPEND FILELIST ${csfile})
	ENDFOREACH()

	set(assemblyinfo "using System\;
using System.Diagnostics\;
using System.Reflection\;
using System.Runtime.CompilerServices\;
using System.Runtime.InteropServices\;
using System.Runtime.Versioning\;

[assembly: AssemblyVersion(\"${Version}\")]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AssemblyTitle(\"${Id}\")]
[assembly: AssemblyDescription(\"${DisplayName}\")]
[assembly: AssemblyCompany(\"TaiwuMods\")]
[assembly: AssemblyProduct(\"${Id}\")]
[assembly: AssemblyCopyright(\"Copyright ©  2018\")]
[assembly: AssemblyFileVersion(\"${Version}\")]
")

     FILE(WRITE "${CMAKE_CURRENT_BINARY_DIR}/${moddir}/Properties/AssemblyInfo.cs" ${assemblyinfo})
	 LIST(APPEND FILELIST "${CMAKE_CURRENT_BINARY_DIR}/${moddir}/Properties/AssemblyInfo.cs")
	#message(STATUS ${moddir} " file list:" ${FILELIST})

	#创建项目
	add_library(${moddir}
		SHARED
		${FILELIST}
		)
	#项目版本 .net 3.5
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v3.5")
	#项目基础引用
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCES
		"System"
		"System.Core"
		"System.Data"
		"System.Xml"
		"System.Runtime.Serialization"
		)
	#AssemblyInfo.cs 处理
	csharp_set_designer_cs_properties(
		"${CMAKE_CURRENT_BINARY_DIR}/${moddir}/Properties/AssemblyInfo.cs"
	)
	
	#dll 引用
	#游戏主dll
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_Assembly-CSharp "${CMAKE_CURRENT_LIST_DIR}\\Assembly-CSharp.dll")
	# harmonypatch工具
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_Harmony12 "${STEAMDIR}${DLLPATH}0harmony12.dll")
	#其他游戏相关dll
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_Newtonsoft.Json "${STEAMDIR}${DLLPATH}Newtonsoft.Json.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine "${STEAMDIR}${DLLPATH}UnityEngine.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.CoreModule "${STEAMDIR}${DLLPATH}UnityEngine.CoreModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.ImageConversionModule "${STEAMDIR}${DLLPATH}UnityEngine.ImageConversionModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.IMGUIModule "${STEAMDIR}${DLLPATH}UnityEngine.IMGUIModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.UI "${STEAMDIR}${DLLPATH}UnityEngine.UI.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.UIElementsModule "${STEAMDIR}${DLLPATH}UnityEngine.UIElementsModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.UIModule "${STEAMDIR}${DLLPATH}UnityEngine.UIModule.dll")
	
	#遍历mod目录下dll，加入引用
	FILE(GLOB_RECURSE MODDLLFILES ${moddir}/*.dll)
	FOREACH(dllfile ${MODDLLFILES})
	  get_filename_component(dllfname ${dllfile} NAME)
	  string(REGEX REPLACE "\\.[^.]*$" "" dllname ${dllfname})
	  message(STATUS "	Add dll type ${dllname}  from ${dllfile}   for mod ${moddir}")
	  set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_${dllname} "${dllfile}")
	ENDFOREACH()
	


	#设置项目引用
	set(ProjectDeps "")
	set(var "")
	foreach(var ${Requirements})
		message(STATUS "	Mod requirements : ${modinfo.Requirements_${var}}")
		target_link_libraries(${moddir} ${modinfo.Requirements_${var}})
	endforeach()

	#清除json
	sbeClearJson(example)
	
	#生成后事件，copy dll到游戏Mods目录
	ADD_CUSTOM_COMMAND(TARGET ${moddir}
		POST_BUILD
		COMMAND copy $(TargetPATH) ${STEAMDIR}Mods/${moddir}/
		COMMAND echo "copy dll file ${moddir}.dll ok"
		COMMENT "copy dll file to steam game" 
	)

ENDMACRO()

#主流程
message(STATUS "Finding All subdir that contains mods")
#mod目录列表
SUBDIRLIST(SUBDIRS ${CMAKE_CURRENT_LIST_DIR})
#挨个处理
FOREACH(subdir ${SUBDIRS})
  CREATEMOD(${subdir})
ENDFOREACH()


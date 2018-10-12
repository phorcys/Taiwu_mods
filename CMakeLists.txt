cmake_minimum_required(VERSION 3.12)
project("Taiwu_Mods" CSharp)

SET(DLLPATH "The Scroll Of Taiwu Alpha V1.0_Data\\Managed\\")

set(CMAKE_CSharp_FLAGS "/langversion:6")

MACRO(SUBDIRLIST result curdir)
  #message(STATUS "search sub dir in:" ${curdir})
  FILE(GLOB children RELATIVE ${curdir} ${curdir}/*)
  SET(dirlist "")
  FOREACH(child ${children})
    IF(IS_DIRECTORY ${curdir}/${child} AND  (NOT ${child} STREQUAL "build") AND (NOT ${child} STREQUAL ".git"))
	  message(STATUS "Found Mod: " ${child})
      LIST(APPEND dirlist ${child})
    ENDIF()
  ENDFOREACH()
  SET(${result} ${dirlist})
ENDMACRO()


MACRO(CREATEMOD moddir)
  SET(FILELIST "")
  FILE(GLOB_RECURSE FILES ${moddir}/*.cs)
  FOREACH(csfile ${FILES})
      LIST(APPEND FILELIST ${csfile})
  ENDFOREACH()
  
  #message(STATUS ${moddir} " file list:" ${FILELIST})
  
  add_library(${moddir}
			SHARED
			${FILELIST}
			)

  set_property(TARGET ${moddir} PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v3.5")

  set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCES
	"System"
	"System.Core"
	"System.Data"
	"System.Xml"
	"System.Runtime.Serialization"
	)
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_Newtonsoft.Json "${STEAMDIR}${DLLPATH}Newtonsoft.Json.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_Harmony12 "${STEAMDIR}${DLLPATH}0harmony12.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_Assembly-CSharp "${STEAMDIR}${DLLPATH}Assembly-CSharp.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine "${STEAMDIR}${DLLPATH}UnityEngine.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.CoreModule "${STEAMDIR}${DLLPATH}UnityEngine.CoreModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.ImageConversionModule "${STEAMDIR}${DLLPATH}UnityEngine.ImageConversionModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.IMGUIModule "${STEAMDIR}${DLLPATH}UnityEngine.IMGUIModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.UI "${STEAMDIR}${DLLPATH}UnityEngine.UI.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.UIElementsModule "${STEAMDIR}${DLLPATH}UnityEngine.UIElementsModule.dll")
	set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_UnityEngine.UIModule "${STEAMDIR}${DLLPATH}UnityEngine.UIModule.dll")
	FILE(GLOB_RECURSE MODDLLFILES ${moddir}/*.dll)
	
	FOREACH(dllfile ${MODDLLFILES})
	  get_filename_component(dllfname ${dllfile} NAME)
	  string(REGEX REPLACE "\\.[^.]*$" "" dllname ${dllfname})
	  message(STATUS "Add dll type ${dllname}  from ${dllfile}   for mod ${moddir}")
      set_property(TARGET ${moddir} PROPERTY VS_DOTNET_REFERENCE_${dllname} "${dllfile}")
	ENDFOREACH()
	
	ADD_CUSTOM_COMMAND(TARGET ${moddir}
        POST_BUILD
        COMMAND copy $(TargetPATH) ${STEAMDIR}Mods/${moddir}/
        COMMAND echo "copy dll file ${moddir}.dll ok"
        COMMENT "copy dll file to steam game" 
	)

ENDMACRO()

message(STATUS "Finding All subdir that contains mods")

SUBDIRLIST(SUBDIRS ${CMAKE_CURRENT_LIST_DIR})
#message(STATUS "subdir list:" ${SUBDIRS})
FOREACH(subdir ${SUBDIRS})
  CREATEMOD(${subdir})
ENDFOREACH()


##
## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

## This file is based on the work of SimpleITK:
##   https://github.com/SimpleITK/SimpleITK/blob/master/CMake/UseCSharp.cmake

# A CMake Module for finding and using C# (.NET and Mono).
#
# The following variables are set:
#   CSHARP_TYPE - the type of the C# compiler (eg. ".NET" or "Mono")
#   CSHARP_COMPILER - the path to the C# compiler executable (eg. "C:/Windows/Microsoft.NET/Framework/v4.0.30319/csc.exe")
#   CSHARP_VERSION - the version number of the C# compiler (eg. "v4.0.30319")
#
# The following macros are defined:
#   CSHARP_ADD_LIBRARY_BINARY( name source_files* ) - Adds binary library and a target corresponding to it
#   CSHARP_ADD_EXECUTABLE( name source_files* ) - Define C# executable with the given name
#   CSHARP_ADD_GUI_EXECUTABLE( name source_files* ) - Define C# gui executable with the given name
#   CSHARP_ADD_LIBRARY( name source_files* ) - Define C# library with the given name
#
#   CSHARP_ADD_DEPENDENCIES( name references* ) - Define C# dependencies for project with the given name. For now dependencies must be defined after library.
#
#   CSHARP_ADD_PROJECT_META( name (key value)* ) - Define C# project meta data. For now dependencies must be defined after library.
#
# Examples:
#   CSHARP_ADD_EXECUTABLE( MyExecutable.exe "Program.cs" )
#   CSHARP_ADD_DEPENDENCIES( MyExecutable.exe "ref1.dll" "ref2.dll" )
#   CSHARP_ADD_PROJECT_META( MyExecutable.exe "TargetFrameworkVersion" "4.0" "Meta2" "Meta2Value" )
#

# Check something was found
if( NOT CSHARP_COMPILER )
  message( WARNING "A C# compiler executable was not found on your system" )
endif( NOT CSHARP_COMPILER )

# Include type-based USE_FILE
if( CSHARP_TYPE MATCHES ".NET" )
  include( ${DotNetFrameworkSdk_USE_FILE} )
elseif ( CSHARP_TYPE MATCHES "Mono" )
  include( ${Mono_USE_FILE} )
endif ( CSHARP_TYPE MATCHES ".NET" )

include(VisualStudioGenerator)

# Macros

function( CSHARP_ADD_LIBRARY_BINARY name)
  message( STATUS "Copying file: ${name} into ${CMAKE_LIBRARY_OUTPUT_DIR}" )
  get_filename_component(filename "${name}" NAME)
  configure_file( "${name}" "${CMAKE_LIBRARY_OUTPUT_DIR}/${filename}" COPYONLY )

  #FIXME: This is a workaround so that the library is properly added to the
  # projects dependency list.
  add_custom_target(${filename})
  add_custom_command(TARGET ${filename} PRE_BUILD
    COMMAND ${CMAKE_COMMAND} -E
    copy ${name} ${CMAKE_LIBRARY_OUTPUT_DIR}
    )
endfunction( CSHARP_ADD_LIBRARY_BINARY )

macro( CSHARP_ADD_TEST_LIBRARY name )
  get_property(VENDORS_TEST_LOCK GLOBAL PROPERTY VENDORS_TEST_LOCK_PROPERTY)
  set(skip_vendor 0)
  foreach(item IN LISTS VENDORS_TEST_LOCK)
    if (CMAKE_CURRENT_SOURCE_DIR MATCHES "^${item}")
      set(skip_vendor 1)
      break()
    endif()
  endforeach(item)
  if (NOT skip_vendor)
    CSHARP_ADD_PROJECT( "test_library" ${name} ${ARGN} )
  else()
    # Add project to skiped projects
    get_property(AUTO_SKIPED_PROJECTS GLOBAL PROPERTY AUTO_SKIPED_PROJECTS_PROPERTY)
    list(APPEND AUTO_SKIPED_PROJECTS "${name}")
    list(REMOVE_DUPLICATES AUTO_SKIPED_PROJECTS)
    set_property(GLOBAL PROPERTY AUTO_SKIPED_PROJECTS_PROPERTY "${AUTO_SKIPED_PROJECTS}")
  endif()
endmacro( CSHARP_ADD_TEST_LIBRARY )

macro( CSHARP_ADD_LIBRARY name )
  CSHARP_ADD_PROJECT( "library" ${name} ${ARGN} )
endmacro( CSHARP_ADD_LIBRARY )

macro( CSHARP_ADD_EXECUTABLE name )
  CSHARP_ADD_PROJECT( "exe" ${name} ${ARGN} )
endmacro( CSHARP_ADD_EXECUTABLE )

macro( CSHARP_ADD_GUI_EXECUTABLE name )
  CSHARP_ADD_PROJECT( "gui" ${name} ${ARGN} )
endmacro( CSHARP_ADD_GUI_EXECUTABLE )

macro( CSHARP_ADD_DEPENDENCY cur_target depends_on )
  if ( TARGET ${depends_on} )
    #FIXME: note that we use target_name, which doesn't get passed as argument.
    # It works because the macro define all their 'local' vars on the global 
    # scope. Thus another macro defined it. This is fragile.
    list(FIND target_name ${depends_on} idx)
    if (idx GREATER -1)
      MESSAGE(STATUS "  ->Depends on[Target/Project]: ${depends_on}")
    else ()
      MESSAGE(STATUS "  ->Depends on[Target/Vendor]: ${depends_on}")
    endif ()
    add_dependencies( ${cur_target} ${depends_on} )
  else ( )
    MESSAGE(STATUS "  ->Depends on[External]: ${depends_on}")
  endif ( TARGET ${depends_on} )
endmacro( CSHARP_ADD_DEPENDENCY )

# Private macro
macro( CSHARP_ADD_PROJECT type name )
  set( sources )
  set( sources_dep )

  if( ${type} MATCHES "library" )
    set( output_type "library" )
    set( TYPE_UPCASE "LIBRARY" )
  elseif( ${type} MATCHES "exe" )
    set( output_type "exe" )
    set( TYPE_UPCASE "RUNTIME" )
  elseif( ${type} MATCHES "gui" )
    set( output_type "winexe" )
    set( TYPE_UPCASE "RUNTIME" )
  elseif( ${type} MATCHES "test_library" )
    set( output_type "library" )
    set( TYPE_UPCASE "LIBRARY" )
  endif( ${type} MATCHES "library" )

  # Step through each argument
  foreach( it ${ARGN} )
    # Argument is a source file
    if( EXISTS ${it} )
      list( APPEND sources ${it} )
      list( APPEND sources_dep ${it} )
    elseif( ${it} MATCHES "AssemblyInfo.cs" )
      list( APPEND sources ${it} )
      list( APPEND sources_dep ${it} )
    elseif( ${it} MATCHES "[*]" )
      # We try to expand wildcards
      FILE( GLOB it_glob ${it} )
      list( APPEND sources ${it} )
      list( APPEND sources_dep ${it_glob} )
    endif( )
  endforeach( )
  # Check we have at least one source
  list( LENGTH sources_dep sources_length )
  if ( ${sources_length} LESS 1 )
    MESSAGE( SEND_ERROR "No C# sources were specified for ${type} ${name}" )
  endif ()
  list( SORT sources_dep )

  # Generate project GUID
  find_program(guid_gen NAMES ${CMAKE_RUNTIME_OUTPUT_DIR}/guid.exe)
  if( NOT guid_gen )
    set( guid_src "${CMAKE_RUNTIME_OUTPUT_DIR}/guid.cs" )
    set( guid_gen "${CMAKE_RUNTIME_OUTPUT_DIR}/guid.exe" )
    file(TO_NATIVE_PATH "${guid_src}" guid_src)
    file(TO_NATIVE_PATH "${guid_gen}" guid_gen)
    file(WRITE ${guid_src} "class GUIDGen { static void Main() { System.Console.Write(System.Guid.NewGuid().ToString().ToUpper()); } }" )
    execute_process(
      WORKING_DIRECTORY "${CMAKE_RUNTIME_OUTPUT_DIR}"
      COMMAND ${CSHARP_COMPILER} /t:exe /out:guid.exe ${guid_src}
    )
  endif ( )
  execute_process(COMMAND ${CSHARP_INTERPRETER} ${guid_gen} OUTPUT_VARIABLE proj_guid )

  # Save project info in global properties
  set_property(GLOBAL APPEND PROPERTY target_name_property "${name}")
  set_property(GLOBAL APPEND PROPERTY target_type_property "${type}")
  set_property(GLOBAL APPEND PROPERTY target_output_type_property "${output_type}")
  set_property(GLOBAL APPEND PROPERTY target_out_property "${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name}")
  set_property(GLOBAL APPEND PROPERTY target_guid_property "${proj_guid}")
  # The implementation relies on fixed numbering. I.e. every property should be
  # set for each target in order CMAKE and VS sln generation to work. In the
  # case of references and test cases we have to insert sort-of empty property
  # for each target. In the case where the current target has test cases or more
  # references we have to edit the string at that position.
  set_property(GLOBAL APPEND PROPERTY target_refs_property "#System.dll")
  set_property(GLOBAL APPEND PROPERTY target_metas_key_property "#")
  set_property(GLOBAL APPEND PROPERTY target_metas_value_property "#")
  set_property(GLOBAL APPEND PROPERTY target_tests_property "#")
  set_property(GLOBAL APPEND PROPERTY target_test_results_property "#")
  # Replace the ; with # in the list of sources. Thus the list becomes
  # "flattened". This is useful when we append the sources for another target
  # and then it will become a list of target sources.
  # Eg. #target1_src1.cs#target1_src2#...;#target2_src1.cs#target2_src2#...
  string(REPLACE ";" "#" s "${sources}")
  set_property(GLOBAL APPEND PROPERTY target_sources_property "#${s}")
  string(REPLACE ";" "#" sd "${sources_dep}")
  set_property(GLOBAL APPEND PROPERTY target_sources_dep_property "#${sd}")
  set_property(GLOBAL APPEND PROPERTY target_src_dir_property "${CMAKE_CURRENT_SOURCE_DIR}")
  set_property(GLOBAL APPEND PROPERTY target_bin_dir_property "${CMAKE_CURRENT_BINARY_DIR}")
  STRING( REGEX REPLACE "(\\.dll)[^\\.dll]*$" "" name_we ${name} )
  STRING( REGEX REPLACE "(\\.exe)[^\\.exe]*$" "" name_we ${name_we} )
  set_property(GLOBAL APPEND PROPERTY target_proj_file_property "${CMAKE_CURRENT_BINARY_DIR}/${name_we}.csproj")
  set_property(GLOBAL APPEND PROPERTY target_generate_proj_file_property TRUE)

endmacro( CSHARP_ADD_PROJECT )

# Define dependencies macro
macro( CSHARP_ADD_DEPENDENCIES name )

  set(refs)

  # Step through each argument
  foreach( it ${ARGN} )
    list( APPEND refs ${it} )
  endforeach( )

  # Save project references info in global properties
  get_property(target_name GLOBAL PROPERTY target_name_property)
  get_property(target_refs GLOBAL PROPERTY target_refs_property)
  list(FIND target_name ${name} idx)
  if (idx GREATER -1)
    if (NOT ("${refs}" STREQUAL ""))
      string(REPLACE ";" "#" r "${refs}")
      get_property(target_refs GLOBAL PROPERTY target_refs_property)
      list(GET target_refs ${idx} old_refs)
      list(INSERT target_refs ${idx} "${old_refs}#${r}")
      math(EXPR idx "${idx}+1")
      list(REMOVE_AT target_refs ${idx})
      set_property(GLOBAL PROPERTY target_refs_property "${target_refs}")
    endif()
  else ()
    get_property(AUTO_SKIPED_PROJECTS GLOBAL PROPERTY AUTO_SKIPED_PROJECTS_PROPERTY)
    list(FIND AUTO_SKIPED_PROJECTS ${name} auto_skiped)
    if (auto_skiped EQUAL -1)
      message(WARNING "Project ${name} was not defined!?")
    endif()
  endif ()

endmacro( CSHARP_ADD_DEPENDENCIES )

# Find Pkg config to expand the packages coming in the style pkg:package
macro ( CSHARP_EXPAND_PACKAGE package_name expanded_package)

# We need to call something like that: 
#[PKG_CONFIG_PATH=/Library/Frameworks/Mono.framework/Versions/Current/lib/pkgconfig/] pkg-config --libs gtk-sharp-2.0

find_package( PkgConfig REQUIRED )
unset(expanded_package)
if(APPLE)
  set(ENV{PKG_CONFIG_PATH} "${CSHARP_MONO_FRAMEWORK}/Versions/Current/lib/pkgconfig/")
endif()
set(command "${PKG_CONFIG_EXECUTABLE}")
execute_process(COMMAND ${command}  --libs ${package_name} OUTPUT_VARIABLE output_var)

#Remove the trailing new line.
string(REPLACE "\n" "" output_var "${output_var}")

set(${expanded_package} ${output_var})
unset(output_var) #FIXME: We really need functions instead of macros - they have scope
endmacro( CSHARP_EXPAND_PACKAGE )

# Resolve dependencies
macro( CSHARP_RESOLVE_DEPENDENCIES )
  # Read global solution info lists
  get_property(target_name GLOBAL PROPERTY target_name_property)
  get_property(target_type GLOBAL PROPERTY target_type_property)
  get_property(target_output_type GLOBAL PROPERTY target_output_type_property)
  get_property(target_out GLOBAL PROPERTY target_out_property)
  get_property(target_sources GLOBAL PROPERTY target_sources_property)
  get_property(target_sources_dep GLOBAL PROPERTY target_sources_dep_property)
  get_property(target_refs GLOBAL PROPERTY target_refs_property)
  get_property(target_metas_key GLOBAL PROPERTY target_metas_key_property)
  get_property(target_metas_value GLOBAL PROPERTY target_metas_value_property)
  get_property(target_bin_dir GLOBAL PROPERTY target_bin_dir_property)
  get_property(target_should_not_skip GLOBAL PROPERTY target_generate_proj_file_property)
  get_property(target_mod_version GLOBAL PROPERTY target_mod_version_property)
  get_property(target_postbuild_cmds GLOBAL PROPERTY target_postbuild_cmds_property)

  # Replace the references that are packages.
  while("${target_refs}" MATCHES "#pkg:")
    STRING(REGEX REPLACE ".*#pkg:([^#]+).*" "\\1" package "${target_refs}")
    if(package)
      CSHARP_EXPAND_PACKAGE(${package} expansion)
      MESSAGE(STATUS "Expanding package ${package} into [${expansion}]")
      STRING(REGEX REPLACE "-r:" "#" expansion "${expansion}" )
      STRING(REGEX REPLACE " " "" expansion "${expansion}")
      STRING(REGEX REPLACE "#pkg:${package}" "${expansion}" target_refs "${target_refs}" )
    endif()
  endwhile()

  # Store the expanded list
  set_property(GLOBAL PROPERTY target_refs_property "${target_refs}")

  # Now we can replace # with #/reference in the local variable.
  string(REPLACE "#" "#/reference:" target_refs "${target_refs}")

  # Define custom targets and commands
  set( i 0 )
  foreach( name ${target_name} )
    list( GET target_should_not_skip ${i} should_not_skip )

    if (TARGET "${name}")
      # Target is already defined
    elseif (should_not_skip)
      list( GET target_type ${i} type )
      list( GET target_output_type ${i} output_type )
      list( GET target_out ${i} out )
      list( GET target_bin_dir ${i} bin_dir )
	  list( GET target_mod_version ${i} mod_version )
	  list( GET target_postbuild_cmds ${i} postbuild_cmds )
	  
      # Sources
      list( GET target_sources ${i} s )
      string(SUBSTRING "${s}" 1 -1 s)
      string(REPLACE "#" ";" sources "${s}")

      # Sources dep-s
      list( GET target_sources_dep ${i} sd )
      string(SUBSTRING "${sd}" 1 -1 sd)
      string(REPLACE "#" ";" sources_dep "${sd}")

      # References
      list( GET target_refs ${i} r )

      string(SUBSTRING "${r}" 1 -1 r)
      string(REPLACE "#" ";" refs "${r}")

      # Project meta data
      list( GET target_metas_key ${i} mk )
      list( GET target_metas_value ${i} mv )
      string(SUBSTRING "${mk}" 1 -1 mk)
      string(REPLACE "#" ";" metas_key "${mk}")
      string(SUBSTRING "${mv}" 1 -1 mv)
      string(REPLACE "#" ";" metas_value "${mv}")

      # Separate resources
      set(separated_sources)
      set(separated_embd_resources)
      set(separated_copy_resources)
      foreach(s ${sources})
        if ("${s}" MATCHES "\\.(stetic)$")
          # Expand wildcards
          FILE( GLOB s_glob ${s} )
          foreach (res ${s_glob})
            get_filename_component(res_name "${res}" NAME)
            list(APPEND separated_embd_resources "/resource:${res},${res_name}")
          endforeach()
        #elseif ("${s}" MATCHES "\\.(config)$")
        #  list(APPEND separated_copy_resources "${s}")
        else()
          list(APPEND separated_sources "${s}")
        endif()
      endforeach()

      # Process references
      set(processed_refs)
      foreach (r ${refs})
        string(REGEX MATCH "^[^,]*," s "${r}")
        if (s)
          string(REGEX MATCH "^[^,]*" s "${s}")
          list(APPEND processed_refs "${s}")
        else()
          list(APPEND processed_refs "${r}")
        endif()
      endforeach()

      # Generate AssemblyInfo.cs
      MESSAGE( STATUS "Generating AssemblyInfo.cs for ${name}" )
      set(VAR_Project_Title "${Project_Title}")
      set(VAR_Project_Description "${Project_Description}")
      string(SUBSTRING "${SolidOpt_LastDate}" 0 4 SolidOpt_LastYear)
      # Project meta data
      set(meta_idx 1)
      foreach ( key ${metas_key} )
        list(GET metas_value ${meta_idx} val)
        math(EXPR meta_idx "${meta_idx}+1")
        set(VAR_Project_${key} "${val}")
      endforeach(key)
      # Configure AssemblyInfo.cs
      configure_file(
        ${CMAKE_MODULE_PATH}/AssemblyInfo.cs.in
        ${bin_dir}/AssemblyInfo.cs
        @ONLY
      )

      # Add custom target and command
      get_filename_component(out_name "${out}" NAME)
      get_filename_component(out_dir "${out}" PATH)
      if ("${CSHARP_PLATFORM}" MATCHES "anycpu")
        set(platform_param "")
      else()
        set(platform_param "/platform:${CSHARP_PLATFORM}")
      endif()
      MESSAGE( STATUS "Adding C# ${type} ${name}: '${CSHARP_COMPILER} /t:${output_type} /out:${out_name} /doc:${out}.xml ${platform_param} ${CSBUILDFLAGS} ${CSHARP_SDK_COMPILER} ${separated_embd_resources} ${separated_sources} ${processed_refs}'" )
      # Transform the ;-separated lists into ' '-separated. This helps copy paste of the command on the terminal
      set(ESCAPED_COMMENT "Compiling C# ${type} ${name}: '${CSHARP_COMPILER} /t:${output_type} /out:${out_name} ${platform_param} ${CSBUILDFLAGS} ${CSHARP_SDK_COMPILER} ${separated_embd_resources} ${separated_sources} ${processed_refs}'")
      string(REPLACE ";" " " ESCAPED_COMMENT "${ESCAPED_COMMENT}")
	  string(REGEX REPLACE "\\.[^.]*$" "" modname ${out_name})
      add_custom_command(
        COMMENT "${ESCAPED_COMMENT}"
        OUTPUT ${out}
        COMMAND ${CSHARP_COMPILER}
        ARGS /t:${output_type} /out:${out} /doc:${out}.xml ${platform_param} ${CSBUILDFLAGS} ${CSHARP_SDK_COMPILER} ${separated_embd_resources} ${separated_sources} ${processed_refs} && mkdir -p ${out_dir}/../Mods/${modname} && cp -rf ${out} ${out_dir}/../Mods/${modname} | true && rsync -am --exclude "*.cs" --exclude "*.resx" --exclude ".\\*" --exclude-from=${CMAKE_CURRENT_LIST_DIR}/${modname}/.modignore ${CMAKE_CURRENT_LIST_DIR}/${modname}/ ${out_dir}/../Mods/${modname}/ | true && cd ${out_dir}/../Mods/ && zip -r ${modname}-${mod_version}.zip ${modname} && rm -rf ${modname}
        WORKING_DIRECTORY ${out_dir}
        DEPENDS ${sources_dep}
      )
      add_custom_target(
        ${name} ALL
        DEPENDS ${out}
        SOURCES ${sources_dep}
      )
    endif()

    # Add a test if target is Test Library - nunit console test runner
    if (type STREQUAL "test_library")
      set(CSHARP_INTEPRETER_INVOCATION ${CSHARP_INTERPRETER})
      if ( UNIX )
        # We need to specify runtime 4.0 to make our own nunit-console work.
        set(CSHARP_INTEPRETER_INVOCATION ${CSHARP_INTERPRETER} --runtime=v4.0)
      endif()
      set(run_test "${name}" ${CSHARP_INTEPRETER_INVOCATION}  "${NUNIT_CONSOLE}" "${out}")
      MESSAGE(STATUS "Add test ${run_test}")

      ADD_TEST(${run_test})

      # Since nunit-console is in bin and tests are in lib we need to add those
      # folders to the MONO_PATH. FIXME: .NET?
      message( STATUS "Configuring ctest environment by adding ${CMAKE_LIBRARY_OUTPUT_DIR} and ${CMAKE_BINARY_DIR}/bin to MONO_PATH" )
      SET_PROPERTY(TEST ${name} PROPERTY
        ENVIRONMENT "MONO_PATH=${CMAKE_LIBRARY_OUTPUT_DIR}:${CMAKE_BINARY_DIR}/bin"
      )
    endif()

    math(EXPR i "${i}+1")
  endforeach( )

  # Resolving all target dependencies
  set( i 0 )
  foreach( name ${target_name} )
    list( GET target_type ${i} type )
    list( GET target_refs ${i} r )
    string(SUBSTRING "${r}" 1 -1 r)
    string(REPLACE "#" ";" refs "${r}")
    MESSAGE( STATUS "Resolving dependencies for ${type}: ${name}" )
    foreach( it ${refs} )
      # Get the filename only (no slashes)
      get_filename_component(filename ${it} NAME)
      csharp_add_dependency(${name} ${filename})
    endforeach( )

    math(EXPR i "${i}+1")
  endforeach( )

endmacro( CSHARP_RESOLVE_DEPENDENCIES )

# Define project meta data macro
macro( CSHARP_ADD_PROJECT_META name )
  set(metas_key)
  set(metas_value)

  set(flag TRUE)

  # Step through each argument
  foreach( it ${ARGN} )
    if (flag)
      list( APPEND metas_key ${it} )
      set(flag FALSE)
    else()
      list( APPEND metas_value ${it} )
      set(flag TRUE)
    endif()
  endforeach( )

  # Save project meta data in global properties
  get_property(target_name GLOBAL PROPERTY target_name_property)
  get_property(target_metas_key GLOBAL PROPERTY target_metas_key_property)
  get_property(target_metas_value GLOBAL PROPERTY target_metas_value_property)
  list(FIND target_name ${name} idx)
  if (idx GREATER -1)
    if (NOT ("${metas_key}" STREQUAL ""))
      string(REPLACE ";" "#" mk "${metas_key}")
      get_property(target_metas_key GLOBAL PROPERTY target_metas_key_property)
      list(GET target_metas_key ${idx} old_metas_key)
      list(INSERT target_metas_key ${idx} "${old_metas_key}#${mk}")
      string(REPLACE ";" "#" mv "${metas_value}")
      get_property(target_metas_value GLOBAL PROPERTY target_metas_value_property)
      list(GET target_metas_value ${idx} old_metas_value)
      list(INSERT target_metas_value ${idx} "${old_metas_value}#${mv}")
      math(EXPR idx "${idx}+1")
      list(REMOVE_AT target_metas_key ${idx})
      set_property(GLOBAL PROPERTY target_metas_key_property "${target_metas_key}")
      list(REMOVE_AT target_metas_value ${idx})
      set_property(GLOBAL PROPERTY target_metas_value_property "${target_metas_value}")
    endif()
  else ()
    get_property(AUTO_SKIPED_PROJECTS GLOBAL PROPERTY AUTO_SKIPED_PROJECTS_PROPERTY)
    list(FIND AUTO_SKIPED_PROJECTS ${name} auto_skiped)
    if (auto_skiped EQUAL -1)
      message(WARNING "Project ${name} was not defined!?")
    endif()
  endif ()

endmacro( CSHARP_ADD_PROJECT_META )

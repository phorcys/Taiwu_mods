#
# A CMake Module for using MSBuild.
#
# The following macros are set:
#   CSHARP_ADD_MSBUILD_PROJECT
#
# Copyright (c) SolidOpt Team
#

function( CSHARP_ADD_MSBUILD_PROJECT filename )

  # Check for optional filename
  get_filename_component(absolute_filename "${filename}" ABSOLUTE)
  file(RELATIVE_PATH filename_tail ${CMAKE_CURRENT_SOURCE_DIR} ${absolute_filename})
  if (NOT EXISTS "${filename}")
    #set(filename "${ARGV1}")
    get_filename_component(filename "${ARGV1}" REALPATH)
  endif()

  # Check for some wrong function usage
  get_filename_component(name ${filename} NAME)
  if ( "${name}" MATCHES "^.*\\.dll$" )
    MESSAGE(FATAL_ERROR "Do not use CSHARP_ADD_MSBUILD_PROJECT with dlls. For dlls use CSHARP_ADD_LIBRARY_BINARY instead.")
  endif()

  STRING( REGEX REPLACE "(\\.csproj)[^\\.csproj]*$" "" name_we ${name} )
  STRING( REGEX REPLACE "(\\.sln)[^\\.sln]*$" "" name_we ${name_we} )

  # Some of the csproj have names after the outputs they generate. Eg:
  # nunit.core.dll.csproj and nunit-console.exe.csproj. 
  # We need to strip these so that the build system can add 'sane' targets and 
  # we could do "make nunit.core.dll' and not 'make nunit.core.dll.dll'
  STRING( REGEX REPLACE "(\\.dll)[^\\.dll]*$" "" name_we ${name_we} )
  STRING( REGEX REPLACE "(\\.exe)[^\\.exe]*$" "" name_we ${name_we} )

  # Copy and adapt the file to the CMAKE setup
  file (READ "${filename}" CSPROJ_FILE)
  # We need to extract:
  #   - output_type
  #   - proj_guid
  #   - type
  string(REGEX REPLACE 
    "(.*<ProjectGuid>{)(.*)(}</ProjectGuid>.*)" "\\2" proj_guid "${CSPROJ_FILE}"
    )

  string(REGEX REPLACE
    "(.*<OutputType>)(.*)(</OutputType>.*)" "\\2" output_type "${CSPROJ_FILE}"
    )
  set(type "${output_type}")
  string(TOLOWER "${output_type}" output_type)
  string(TOUPPER ${output_type} TYPE_UPCASE)
  set( output "${output_type}" )
  if(TYPE_UPCASE STREQUAL "LIBRARY")
    set( output "dll" )
    #TODO: We should think of making CMAKE_LIBRARY_OUTPUT_DIR and 
    # CMAKE_RUNTIME_OUTPUT_DIR. 
    # Thus all the Exe->Runtime translations will be removed.
  elseif(TYPE_UPCASE STREQUAL "EXE")
    set( TYPE_UPCASE "RUNTIME" )
  endif()

  # Check and skip target if custom target already exists
  if (TARGET "${name_we}.${output}")
    MESSAGE( STATUS "Target project ${filename} already exists - Skip it." )
    return()
  endif()

  # Add custom target and command
  MESSAGE( STATUS "Adding project ${filename} for MSBuild-ing." )

  #FIXME: We know that we only build vendors with MSBuild. Thus we can afford to 
  # decrease the warning level, because we cannot fully control the vendor 
  # warnings.
  string(REGEX REPLACE 
    "<WarningLevel>[0-4]</WarningLevel>" 
    "<WarningLevel>1</WarningLevel>" 
    CSPROJ_FILE "${CSPROJ_FILE}"
    )

  # We need to replace some the csproj file with the currently active cmake
  # configuration. Thus it is better to copy the file in our build folder and
  # make the substitutions there. 
  # Settings that should be changed are:
  #   - OutputPath
  #   - DocumentationFile
  #   - TargetFrameworkVersion
  #   - DebugType
  #   - <Compile Include="AssemblyInfo.cs" ...
  #   - <EmbeddedResource Include=" ...
  #   - <Content Include="..
  #   - <None Include="..." ...
  #   - <AssemblyOriginatorKeyFile>...

  # Extract original OutputPath
  string(REGEX MATCH "<OutputPath>[^<]*</OutputPath>" old_outputpath "${CSPROJ_FILE}")
  string(REPLACE "<OutputPath>" "" old_outputpath "${old_outputpath}")
  string(REPLACE "</OutputPath>" "" old_outputpath "${old_outputpath}")
  # Replace old OutputPath with cmake output dir
  string(REGEX REPLACE
    "<OutputPath>[^<]*</OutputPath>"
    "<OutputPath>${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}</OutputPath>"
    CSPROJ_FILE "${CSPROJ_FILE}"
  )
  # Extract original DocumentationFile
  string(REGEX MATCH "<DocumentationFile>[^<]*</DocumentationFile>" docfile "${CSPROJ_FILE}")
  string(REPLACE "<DocumentationFile>" "" docfile "${docfile}")
  string(REPLACE "</DocumentationFile>" "" docfile "${docfile}")
  string(REGEX REPLACE "[\r\n\t ]" "" docfile "${docfile}")
  # Remove original output path form doc file name
  string(REPLACE "${old_outputpath}" "" docfile "${docfile}")
  if (docfile)
    string(REGEX REPLACE
      "<DocumentationFile>[^<]*</DocumentationFile>"
      "<DocumentationFile>${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}${docfile}</DocumentationFile>"
      CSPROJ_FILE "${CSPROJ_FILE}"
    )
  endif()

  string(REGEX REPLACE
    "<TargetFrameworkVersion>.*</TargetFrameworkVersion>"
    "<TargetFrameworkVersion>v${CSHARP_FRAMEWORK_VERSION}</TargetFrameworkVersion>"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  get_filename_component(filename_path "${filename}" PATH)
  file(RELATIVE_PATH rel_filename_path ${CMAKE_CURRENT_SOURCE_DIR} ${filename_path})
  string(REPLACE "/" "\\" msbuild_path "${CMAKE_CURRENT_SOURCE_DIR}/${rel_filename_path}/")
  string(REPLACE
    "<Compile Include=\""
    "<Compile Include=\"${msbuild_path}"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<EmbeddedResource Include=\""
    "<EmbeddedResource Include=\"${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<Content Include=\""
    "<Content Include=\"${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<None Include=\""
    "<None Include=\"${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REPLACE
    "<AssemblyOriginatorKeyFile>"
    "<AssemblyOriginatorKeyFile>${msbuild_path}\\"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )
  string(REGEX REPLACE 
    "(.*<Project.*ToolsVersion=\")([0-9]\\.[0-9])(\".*>)" 
    "\\1${CSHARP_FRAMEWORK_VERSION}\\3"
    CSPROJ_FILE "${CSPROJ_FILE}"
    )

  # Save the new csproj file
  # Create the missing directories
  set(new_csproj_filename "${CMAKE_CURRENT_BINARY_DIR}/${filename_tail}")
  file(WRITE "${new_csproj_filename}" "${CSPROJ_FILE}")

  # Save project info in global properties
  # Put the dll instead of ${name} because the name ends with csproj and the
  # dependency checker expects to be an output type.
  set_property(GLOBAL APPEND PROPERTY target_name_property "${name_we}.${output}")
  set_property(GLOBAL APPEND PROPERTY target_type_property "${type}")
  set_property(GLOBAL APPEND PROPERTY target_output_type_property "${output_type}")
  set_property(GLOBAL APPEND PROPERTY target_out_property "${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name}")
  set_property(GLOBAL APPEND PROPERTY target_guid_property "${proj_guid}")
  # The implementation relies on fixed numbering. I.e. every property should be
  # set for each target in order CMAKE and VS sln generation to work. In the
  # case of references and test cases we have to insert sort-of empty property
  # for each target. In the case where the current target has test cases or more
  # references we have to edit the string at that position.
  set_property(GLOBAL APPEND PROPERTY target_refs_property "#")
  set_property(GLOBAL APPEND PROPERTY target_metas_key_property "#")
  set_property(GLOBAL APPEND PROPERTY target_metas_value_property "#")
  set_property(GLOBAL APPEND PROPERTY target_tests_property "#")
  set_property(GLOBAL APPEND PROPERTY target_test_results_property "#")
  # No need of sources
  set_property(GLOBAL APPEND PROPERTY target_sources_property "#")
  # No need of source dependencies
  set_property(GLOBAL APPEND PROPERTY target_sources_dep_property "#")
  set_property(GLOBAL APPEND PROPERTY target_src_dir_property "${CMAKE_CURRENT_SOURCE_DIR}")
  set_property(GLOBAL APPEND PROPERTY target_bin_dir_property "${CMAKE_CURRENT_BINARY_DIR}")
  # Empty will signal that the csproj is already built
  set_property(GLOBAL APPEND PROPERTY target_proj_file_property "${new_csproj_filename}")
  set_property(GLOBAL APPEND PROPERTY target_generate_proj_file_property FALSE)
  #set_target_properties("${name_we}.${output}" PROPERTIES csproj "${new_csproj_filename}")


  #
  #list(APPEND MSBUILDFLAGS "/p:OutputPath=${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}")
  string(REPLACE ";" " " MSBUILDFLAGS "${MSBUILDFLAGS}")
  add_custom_command(
    COMMENT "MSBuilding: ${MSBUILD} ${MSBUILDFLAGS} ${new_csproj_filename}"
    OUTPUT ${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name_we}.${output}
    COMMAND ${MSBUILD}
    ARGS ${MSBUILDFLAGS} ${new_csproj_filename}
    WORKING_DIRECTORY ${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}
    )
  add_custom_target(
    "${name_we}.${output}" ALL
    DEPENDS ${CMAKE_${TYPE_UPCASE}_OUTPUT_DIR}/${name_we}.${output}
    SOURCES ${new_csproj_filename}
    )

endfunction( CSHARP_ADD_MSBUILD_PROJECT )

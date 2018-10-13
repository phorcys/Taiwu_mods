#
# A CMake Module for finding and using the prerequisites needed for running unit 
# tests.
#
# The following variables are set:
#   CSC - The current CSharp Compiler
#   ILASM - The current version of ILASM
#  
#
# Copyright (c) SolidOpt Team
#
if( WIN32 )
  list( GET CSHARP_DOTNET_VERSIONS 0 newest_csharp_dotnet_version )
endif( )

find_program(ILASM NAMES ilasm ${csharp_dotnet_framework_dir}/${newest_csharp_dotnet_version}/msbuild.exe)
if ( NOT ILASM )
  message(FATAL_ERROR "ILASM not found.")
else( )
  message(STATUS "ILASM found : ${ILASM}")
endif( )

if (NOT NUNIT_CONSOLE)
  set(NUNIT_CONSOLE "${CMAKE_CURRENT_BINARY_DIR}/bin/nunit-console.exe")
  message(STATUS "NUNIT-CONSOLE : ${NUNIT_CONSOLE}")
endif()

set(CSC ${CSHARP_COMPILER})
if ( NOT CSC )
  message(FATAL_ERROR "CSC not found.")
else( )
  message(STATUS "CSC found : ${CSC}")
endif( )

# Set the USE_FILE path
get_filename_component( current_list_path ${CMAKE_CURRENT_LIST_FILE} PATH )
set( TestingPrerequisites_USE_FILE ${current_list_path}/UseTestingPrerequisites.cmake )

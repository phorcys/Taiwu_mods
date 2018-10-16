##
## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

#
# A CMake Module for finding MSBuild.
#
# The following variables are set:
#   MSBUILD
#

if( WIN32 )
  list( GET CSHARP_DOTNET_VERSIONS 0 newest_csharp_dotnet_version )
endif( )

find_program(MSBUILD NAMES xbuild ${csharp_dotnet_framework_dir}/${newest_csharp_dotnet_version}/msbuild.exe)
if ( NOT MSBUILD )
  message(FATAL_ERROR "MSBuild not found.")
else( )
  message(STATUS "MSBuild found : ${MSBUILD}")
endif( )

# Set the USE_FILE path
get_filename_component( current_list_path ${CMAKE_CURRENT_LIST_FILE} PATH )
set( MSBuild_USE_FILE ${current_list_path}/UseMSBuild.cmake )

##
## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

configure_file(
  ${CMAKE_MODULE_PATH}/BuildInfo.cs.in
  ${CMAKE_CURRENT_BINARY_DIR}/BuildInfo.cs
  @ONLY
  )

csharp_add_library(
  SolidOpt.BuildInformation.dll
  ${CMAKE_CURRENT_BINARY_DIR}/BuildInfo.cs
  ${CMAKE_CURRENT_BINARY_DIR}/AssemblyInfo.cs
  )

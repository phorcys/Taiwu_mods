# $Id$
# It is part of the SolidOpt Copyright Policy (see Copyright.txt)
# For further details see the nearest License.txt

# This defines SolidOpt versioning.
# The following variables are set:
#   SolidOpt_Major - Framework major version
#   SolidOpt_Minor - Framework minor version
#   SolidOpt_Revision - Svn working copy revision.
#   SolidOpt_LastDate - Svn working copy last revision date.
#   SolidOpt_Version - The framework current version string.

set(SolidOpt_Major 0)
set(SolidOpt_Minor 0)
set(Project_Title "SolidOpt Optimization Framework")
set(Project_Description "SolidOpt is ...")

#TODO: Fix to works with GIT repository
# Get trunk rev number.
find_program(SVN_EXECUTABLE NAMES svn svn.exe DOC "subversion command line client")

macro(SUBVERSION_GET dir variable name)
  execute_process(
    COMMAND ${SVN_EXECUTABLE} info ${dir}
    OUTPUT_VARIABLE ${variable}
    OUTPUT_STRIP_TRAILING_WHITESPACE
    )
  string(REGEX REPLACE "^(.*\n)?${name}: ([^\n]+).*" "\\2" ${variable} "${${variable}}")
endmacro(SUBVERSION_GET)

if (SVN_EXECUTABLE)
  subversion_get(${CMAKE_SOURCE_DIR} SolidOpt_Revision "Revision")
  subversion_get(${CMAKE_SOURCE_DIR} SolidOpt_LastDate "Last Changed Date")
endif()
if (NOT SolidOpt_LastDate)
  set(temp "$Revision$")
  string(REGEX REPLACE "^.?Revision:?.?([0-9]*).*$" "\\1" SolidOpt_Revision "${temp}")
  if (NOT SolidOpt_Revision)
    set (SolidOpt_Revision 0)
  endif ()

  set(temp "$Date$")
  string(REGEX REPLACE "^.?Date:?.?(.*).$" "\\1" SolidOpt_LastDate "${temp}")
  if (NOT SolidOpt_LastDate)
    set (SolidOpt_LastDate "2013-01-01")
  endif ()
endif ()
string(STRIP "${SolidOpt_LastDate}" SolidOpt_LastDate)

set(SolidOpt_Version ${SolidOpt_Major}.${SolidOpt_Minor}.0.${SolidOpt_Revision})

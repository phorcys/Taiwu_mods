##
## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

SET(CTEST_PROJECT_NAME "SolidOpt")
SET(CTEST_NIGHTLY_START_TIME "00:00:00 EST")

IF(NOT DEFINED CTEST_DROP_METHOD)
  SET(CTEST_DROP_METHOD "http")
ENDIF(NOT DEFINED CTEST_DROP_METHOD)

IF(CTEST_DROP_METHOD STREQUAL "http")
  SET(CTEST_DROP_SITE "ci.solidopt.org")
  SET(CTEST_DROP_LOCATION "/CDash/submit.php?project=SolidOpt")
  SET(CTEST_TRIGGER_SITE "")
ENDIF(CTEST_DROP_METHOD STREQUAL "http")

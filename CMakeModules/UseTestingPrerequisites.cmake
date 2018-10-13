## $Id$
## It is part of the SolidOpt Copyright Policy (see Copyright.txt)
## For further details see the nearest License.txt
##

#
# A CMake Module for finding and using the prerequisites needed for running unit
# tests.
#
# The following macros are set:
#   CSHARP_ADD_TEST_CASE - Adds a single test case. It relies on the target 
#                          being defined before the calling that macro.
#  

macro( CSHARP_ADD_TEST_CASE name)
  set(test_cases)
  set(test_results)
  # Step through each argument. Argument is a test source file
  foreach( it ${ARGN} )
    # We need to expand wildcards
    if( EXISTS ${it} )
      list( APPEND test_cases ${it} )
    else()
      FILE( GLOB it_glob ${it} )
      list( APPEND test_cases ${it_glob} )
    endif()
  endforeach( )

  foreach(it ${test_cases})
    get_filename_component(test_case ${it} NAME)
    # Export that variable for the testsuite itself, pointing to the current 
    # test case.
    get_filename_component(TEST_CASE_NAME ${it} NAME_WE)
    set(TEST_CASE ${CMAKE_CURRENT_BINARY_DIR}/${test_case})

    # Consider it.* as result files and copy them over.
    # Expand wildcards first.
    FILE( GLOB test_case_results ${it}.* )
    foreach(result_it ${test_case_results})
      get_filename_component(test_case_result ${result_it} NAME)
      # Add the result files to the list of result files.
      list( APPEND test_results ${result_it} )
    endforeach()
  endforeach()

  # Save project references info in global properties
  # This relies on setting/creating the target first and then calling that macro
  get_property(target_name GLOBAL PROPERTY target_name_property)
  list(FIND target_name ${name} idx)
  if (idx GREATER -1)
    # If we have test cases to add
    get_property(target_tests GLOBAL PROPERTY target_tests_property)
    if (NOT ("${test_cases}" STREQUAL ""))
      string(REPLACE ";" "#" r "${test_cases}")
      list(GET target_tests ${idx} old_tests)
      list(INSERT target_tests ${idx} "${old_tests}#${r}")
      # Use another variable (index) because incrementing inc would cause the
      # second if to fail
      math(EXPR index "${idx}+1")
      list(REMOVE_AT target_tests ${index})
      set_property(GLOBAL PROPERTY target_tests_property "${target_tests}")
    endif()
    # If we have test case results to add
    get_property(target_test_results GLOBAL PROPERTY target_test_results_property)
    if (NOT ("${test_results}" STREQUAL ""))
      string(REPLACE ";" "#" r "${test_results}")
      list(GET target_test_results ${idx} old_test_results)
      list(INSERT target_test_results ${idx} "${old_test_results}#${r}")
      math(EXPR idx "${idx}+1")
      list(REMOVE_AT target_test_results ${idx})
      set_property(GLOBAL PROPERTY target_test_results_property "${target_test_results}")
    endif()
  else ()
    get_property(AUTO_SKIPED_PROJECTS GLOBAL PROPERTY AUTO_SKIPED_PROJECTS_PROPERTY)
    list(FIND AUTO_SKIPED_PROJECTS ${name} auto_skiped)
    if (auto_skiped EQUAL -1)
      message(WARNING "Project ${name} was not defined!?")
    endif()
  endif ()
endmacro( CSHARP_ADD_TEST_CASE )

# $Id$
# It is part of the SolidOpt Copyright Policy (see Copyright.txt)
# For further details see the nearest License.txt

macro(ADD_VENDOR name vendor_path)

  if(NOT EXISTS ${name})
    ### TODO: Remove hard dependency to SolidOpt vendors
    get_filename_component(VENDOR_ABSOLUTE_PATH "${vendor_path}" ABSOLUTE)

    # Add vendor absolute path to global list with vendors to skip when generate CTests
    get_property(VENDORS_TEST_LOCK GLOBAL PROPERTY VENDORS_TEST_LOCK_PROPERTY)
    list(APPEND VENDORS_TEST_LOCK "${VENDOR_ABSOLUTE_PATH}")
    list(REMOVE_DUPLICATES VENDORS_TEST_LOCK)
    set_property(GLOBAL PROPERTY VENDORS_TEST_LOCK_PROPERTY "${VENDORS_TEST_LOCK}")

    # Add subdirecory to build process
    add_subdirectory("${VENDOR_ABSOLUTE_PATH}" "${name}")
  else()
    # Add vendor absolute path to global list with vendors to skip when generate CTests
    get_filename_component(VENDOR_ABSOLUTE_PATH "${name}" ABSOLUTE)
    get_property(VENDORS_TEST_LOCK GLOBAL PROPERTY VENDORS_TEST_LOCK_PROPERTY)
    list(APPEND VENDORS_TEST_LOCK "${VENDOR_ABSOLUTE_PATH}")
    list(REMOVE_DUPLICATES VENDORS_TEST_LOCK)
    set_property(GLOBAL PROPERTY VENDORS_TEST_LOCK_PROPERTY "${VENDORS_TEST_LOCK}")

    # Add subdirecory to build process
    add_subdirectory("${name}")
  endif()

endmacro(ADD_VENDOR)

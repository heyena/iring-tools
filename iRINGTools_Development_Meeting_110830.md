# Attendees #
  * Rob
  * Aswini
  * Koos
  * Rashmi
  * Neha
  * Fang
  * Hahn

# Agenda #
  * Trunk Split

# Trunk Split #
  * What changes need to be made to trunk?
    * EXT JS 4.0
    * Remove Silverlight
    * Service Locator
    * Move Java
  * Why they can't be made until split?
    * break those of use who use trunk for demos
    * we need a functional space to make changes to in 	use code.
    * we have three efforts ongoing
      1. demo, trial and pilot use of 2.2
> 2 major construction of 3.0
    * Why a branch might not be enough?
      * dev environment which is used for demo of 3.0?
      * we have no immediate need to demo 3.0
      * iip environment which is used for testing/demo of stable 2.2
      * should we utilize iringtools now?
    * Actions
      * Split trunk to 2.2.x - Rob - done
      * Change 2.2.x version to 2.2.0 - Rob
      * Change trunk version to 3.0.0 - Rob
      * In dotNet trunk
        * Remove silverlight apps - Koos - done
  * Remove silverlight modules - Koos - done
  * Remove prism lib - Koos - done
  * Remove Silverlight links form HTML pages - Koos - done
  * Remove silverlight junk form build - Koos
  * Remove silverlight external binaries - Koos - done
  * Remove Silverlight verison of other libs - Koos
  * Remove desktop qualifier form other libs - Koos
    * Move ESBServices to java trunk - Hahn - done
  * Move all documentation out of trunk and into dropbox? - Rob
  * Need to release 2.2.0
    * build branch - Hahn - done
    * build 2.2 of java (trunk) - Hahn
    * upload - Koos
    * non-automated deploy verification - Koos
    * testing - Koos
    * relnote - Koos
    * iRINGSandbox Actions
    * showroom - needs to be upgraded to 2.2.x - rohit/devesh/?
    * sdk - need to be removed - fang/aswini/rashmi
    * iip - needs to be upgraded to 2.2.x - fang/aswini/rashmi/neha
    * dev - build script will need to be fixed to support new trunk - Koos
    * tomcat - need to switch to host java trunk not dotNet trunk - Hahn

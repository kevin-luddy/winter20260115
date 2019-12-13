/*
 * This file should be included in every master page.
 *
 * The contents of this file take care of things that would normally have to repeat in the master pages..
 *
 */

// This script is used to prevent cross-frame attacks
if (top.frames.length != 0) {
    top.location = self.document.location;
}

/*
 * This disables external links on the page
 */
function DisableExternalLinks() {
    // check every link on the page
    $("a").each(function () {
        var originalLink = $(this).attr('href');

        // Disable links if they contain:
            // pae-knowledgebase.isgs.lmco.com
            // isgs-gen.external.lmco.com
            // generationsupport.isgs.lmco.com
        if (originalLink != undefined && (
            originalLink.indexOf('pae-knowledgebase.isgs.lmco.com') > -1 ||
            originalLink.indexOf('isgs-gen.external.lmco.com') > -1)) {

                var originalText = $(this).text();
                var newText = originalText + ' LINK AVAILABLE IN UNCLASSIFIED ONLY: ' + originalLink + ' ';

                // set the new text value
                $(this).text(newText);

                // removes the linkability
                $(this).contents().unwrap();
        }
    });
}

$(function () {
    // if we are in classified environment, we will look for all external links and change them appropriately
    if (disableLinks_Global) {
        // most of the pages this will trigger the change correctly
        $('.main').ajaxComplete(function () {
            DisableExternalLinks();
        });

        // on pages w/ angular, this is needed
        $('.module-content-data').ready(function () {
            DisableExternalLinks();
        });
    }
});
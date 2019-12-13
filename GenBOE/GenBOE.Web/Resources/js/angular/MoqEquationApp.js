// The MOQ equation module.
var moqEquationApp = angular.module('MoqEquationApp', ['ui.bootstrap']);

// Handles interaction with templates stored in the template cache.
moqEquationApp.factory('TemplateService', ['$templateCache', function ($templateCache) {
    return {
        RemoveTrailingNewline: function (templatePath) {
            var template = $templateCache.get(templatePath).replace(/(?:\r\n|\r|\n)/g, '');
            $templateCache.put(templatePath, template);
        }
    };
}]);

// Initialization logic.
moqEquationApp.run(['TemplateService', function (TemplateService) {
    // modify the default bootstrap templates to remove the trailing newline characters, which seem to be conflicting with something
    //TemplateService.RemoveTrailingNewline('uib/template/modal/backdrop.html');
    TemplateService.RemoveTrailingNewline('uib/template/modal/window.html');
    TemplateService.RemoveTrailingNewline('uib/template/tooltip/tooltip-popup.html');
}]);
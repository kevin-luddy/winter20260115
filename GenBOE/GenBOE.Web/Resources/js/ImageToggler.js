// ***************************************************
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2017 Lockheed Martin Corporation
// </copyright>
// ***************************************************

// ---------------------------------------------------------------------------------------------
// Name:         Image Toggler jQuery plugin
//
// Description:  Supports the ability to toggle between two states (enabled or disabled), each
//               represented by its own image file.  Clicking the "active" image triggers the
//               state change, causes updates to the corresponding display elements and invokes
//               the associated event handler.
//
// Events:       onEnabled - Fired when the state becomes set to "enabled"
//               onDisabled - Fired when the state becomes set to "disabled"
//               onDestroy - Fired when the widget is destroyed
// ---------------------------------------------------------------------------------------------
//                                          Change Log
// ---------------------------------------------------------------------------------------------
// DATE         AUTHOR              DESCRIPTION
// ----------   ----------          ------------------------------------------------------------
// 04/24/2012   sjrosent/goodwik1   Initial development
// Oct/Nov 2013 sjrosent            Added event-trigger flag
// 12/04/2014   sjrosent            Changed layout to fix shifting of lock image for long text.
// 12/12/2014   sjrosent            Commented out hover/title processing.
// 09/14/2015   twilson3            Added optional css class for outer container
// ---------------------------------------------------------------------------------------------

(function ($, undefined) {
    $.widget("ui.imageToggler", {
        options: {
            objectCssClass: undefined,          // (Optional) CSS class for the object the toggler is attached to
            imageCssClass: undefined,           // (Optional) CSS class for the toggler image 
            containerCssClass: undefined,       // (Optional) CSS class for the outer container
            enabledImage: undefined,            // Path of the "enabled" image
            enabledImageHoverText: '',          // Hover text for the "enabled" image
            onEnabled: undefined,               // Click event handler for the "enabled" image
            disabledImage: undefined,           // Path of the "disabled" image
            disabledImageHoverText: '',         // Hover text for the "disabled" image
            onDisabled: undefined,              // Click event handler for the "disabled" image
            onDestroy: undefined,               // Event handler called when the toggler is destroyed
            enabled: true,                      // Enabled/disabled flag
            data: undefined                     // (Optional) data for the handler callbacks
        },

        imageElement: undefined,                // local variable to reference the image widget

        _create: function () {
            this.element.attr('role', 'imageToggler');

            var self = this;
            var obj = this.element;  // text input element

            // create the image and point it to the correct file path
            var imageObject = new Image();
            var image = $(imageObject);

            if (this.options.objectCssClass != undefined) {  // apply styling to the object
                obj.addClass(this.options.objectCssClass);
            }

            if (this.options.imageCssClass != undefined) {  // apply styling to the image
                image.addClass(this.options.imageCssClass);
            }

            image.click(function () {  // bind the image's click event handler
                self.toggle();
            });

            //obj.hover(function () {  // bind the input element's hover event handler
            //    self.hover();
            //});

            obj.after(image);  // add the image to the document

            image.wrap('<div />');  // wrap the image in its own div
            obj.wrap('<div />');  // wrap the object in its own div
            var divImage = image.parent('div');
            var divObj = obj.parent('div');
            divObj.css('float', 'left');

            divImage.add(divObj).wrapAll('<div />'); // wrap the image and object divs in a single "container" div
            var innerContainer = divImage.parent('div');
            innerContainer.css('position', 'absolute');
            innerContainer.wrap('<div />');
            var outerContainer = innerContainer.parent('div');
            outerContainer.css('position', 'static').css('height', '16px');  // line height

            this.imageElement = image;

            if (this.options.enabledImage) {
                image.attr('src', this.options.enabledImage);  // apply enabled image so we can get its width

                var imageWidth = image.width();
                var outerContainerWidth = outerContainer.width();

                var objAdjustedWidth = outerContainerWidth - imageWidth - 2;

                obj.css('width', objAdjustedWidth);  // make the input element just big enough to not hide the image

                image.removeAttr('src');  // re-initialize image to unassigned
            }

            if (this.options.containerCssClass != undefined) {  // apply styling to the outer container
                outerContainer.addClass(this.options.containerCssClass);
            }

            this.setEnabled(this.options.enabled, true);  // do not trigger events during initialization
        },

        _init: function () {
            return;
        },

        destroy: function () {
            var obj = this.element;
            var image = this.imageElement;

            var divContainer = image.parent('div').parent('div');

            var originalContainer = divContainer.parent();

            image.unbind('click');  // unbind image click event

            obj.detach();  // separate the underlying object from the (created) wrapper divs
            obj.removeAttr("role");  // remove role attribute

            if (this.options.objectCssClass != undefined) {
                obj.removeClass(this.options.objectCssClass);  // remove CSS class
            }

            // remove the wrapper divs - root container will take children with it
            divContainer.remove();

            originalContainer.append(obj);  // restore the object to its original position within the DOM

            $.Widget.prototype.destroy.apply(this, arguments);

            // call the destroy handler (for custom post-processing of the object)
            if (this.options.onDestroy != undefined) {
                this.options.onDestroy(obj, this.options.data);
            }
        },

        _setOption: function (key, value) {
            switch (key) {
                case "objectCssClass":
                    this.options.objectCssClass = value;
                    break;
                case "imageCssClass":
                    this.options.imageCssClass = value;
                    break;
                case "enabledImage":
                    this.options.enabledImage = value;
                    break;
                case "enabledImageHoverText":
                    this.options.enabledImageHoverText = value;
                    break;
                case "onEnabled":
                    this.options.onEnabled = value;
                    break;
                case "disabledImage":
                    this.options.disabledImage = value;
                    break;
                case "disabledImageHoverText":
                    this.options.disabledImageHoverText = value;
                    break;
                case "onDisabled":
                    this.options.onDisabled = value;
                    break;
                case "enabled":
                    this.setEnabled(value);
                    break;
            }

            $.Widget.prototype._setOption.apply(this, arguments);
        },

        setEnabled: function (value, skipEvents) {
            this.options.enabled = value;
            var triggerEvents = (skipEvents == undefined) ? true : (skipEvents != true);
            var img = this.imageElement;
            if (value) {
                var hoverText = this.options.enabledImageHoverText;
                $(img).attr('src', this.options.enabledImage).attr('title', hoverText);
                if (triggerEvents && this.options.onEnabled != undefined) {
                    this.options.onEnabled(this.element, this.options.data);
                }
            } else {
                var hoverText = this.options.disabledImageHoverText;
                $(img).attr('src', this.options.disabledImage).attr('title', hoverText);
                if (triggerEvents && this.options.onDisabled != undefined) {
                    this.options.onDisabled(this.element, this.options.data);
                }
            }
        },

        isEnabled: function () {
            return this.options.enabled;
        },

        toggle: function () {
            this.setEnabled(!this.options.enabled);
        },

        hover: function () {
            this.element.attr('title', this.element.val());
        },

        hide: function () {
            return;
        },

        show: function () {
            return;
        }

    });  // end $.widget

    $.extend($.ui.imageToggler, {
        version: "1.10.3"
    });

})(jQuery);
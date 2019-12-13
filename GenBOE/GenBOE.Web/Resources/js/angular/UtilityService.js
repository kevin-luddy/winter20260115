angular.module("genboe").factory("utilityService", ['$http', function ($http) {
    var utilityService = {

        /* Gets a Date/Time from a JSON date. */
        getDateStringFromJsonDate: function (jsonDate) {
            if (jsonDate) {
                return new Date(parseInt(jsonDate.replace(/\/Date\((.*?)\)\//gi, "$1")));
            } else {
                return "";
            }
        },

        /* Gets a date string in MM/dd/yyyy format from a JSON date. */
        getMonthDateYearDateStringFromJsonDate: function (jsonDate) {
            if (jsonDate) {
                var date = new Date(parseInt(jsonDate.substr(6)));
                return (date.getMonth() + 1) + "/" + date.getDate() + "/" + date.getFullYear();
            }
            return "";
        },

        /* Gets a date string in MM/dd/yyyy HH:mm:ss AM format from a JSON date. */
        getLocaleDateTimeStringFromJsonDate: function (jsonDate) {
            if (jsonDate) {
                var date = new Date(parseInt(jsonDate.substr(6)));
                return (date.toLocaleDateString() + " " + date.toLocaleTimeString());
            }
            return "";
        },

        /* Initialize TinyMCE to a basic set of options */
        getTinyMceOptions: function (elementName, options, widget) {
            var maxlen = options.maxlen;
            var enableCharCounting = options.enableCharCounting || false;

            // Remove any existing editor with the same ID (elementName).  If we don't, it will sometimes cause problems during partial page refreshes.
            tinymce.EditorManager.execCommand('mceRemoveEditor', true, elementName);

            return {
                cache_suffix: "?v=4.7.13",
                width: '100%',
                statusbar: true,
                menubar: true,
                resize: true,
                height: 200,
                theme: "modern",
                theme_url: '/Scripts/tinymce/themes/modern/theme.min.js',
                skin: 'lightgray',
                skin_url: '/Scripts/tinymce/skins/lightgray',
                plugins: [
                    "advlist autolink link image lists charmap preview hr anchor pagebreak, code",
                    "searchreplace visualblocks visualchars fullscreen insertdatetime nonbreaking",
                    "save table contextmenu directionality template paste textcolor"
                ],
                browser_spellcheck: true,
                paste_data_images: true,
                convert_urls: false,
                paste_block_drop: false,
                paste_retain_style_properties: "all",
                paste_word_valid_elements: "@[style],-strong/b,-em/i,-span,-p,-ol,-ul,-li,-h1,-h2,-h3,-h4,-h5,-h6," +
                    "-table,-tr,-td[colspan|rowspan],-th,-thead,-tfoot,-tbody,-a[href|name],-font[color],sub,sup,strike,br,u",
                style_formats: [
                    {
                        title: "Inline", items: [
                            { title: "Bold", icon: "bold", format: "bold" },
                            { title: "Italic", icon: "italic", format: "italic" },
                            { title: "Underline", icon: "underline", format: "underline" },
                            { title: "Strikethrough", icon: "strikethrough", format: "strikethrough" },
                            { title: "Superscript", icon: "superscript", format: "superscript" },
                            { title: "Subscript", icon: "subscript", format: "subscript" }
                        ]
                    },
                    {
                        title: "Alignment", items: [
                            { title: "Left", icon: "alignleft", format: "alignleft" },
                            { title: "Center", icon: "aligncenter", format: "aligncenter" },
                            { title: "Right", icon: "alignright", format: "alignright" },
                            { title: "Justify", icon: "alignjustify", format: "alignjustify" }
                        ]
                    }
                ],
                toolbar: "insertfile undo redo | styleselect | bold italic underline | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | preview fullpage | forecolor backcolor | table" /*+ " | code"*/,
                visual: false, // turns off visual aid that draws dotted lines around tables with no borders.  With this on pasted in tables from Excel don't display an outer border
                content_css: "/Resources/css/tinymce.css",
                menu: {
                    edit: { title: 'Edit', items: 'undo redo | cut copy paste pastetext | selectall | searchreplace' },
                    insert: { title: 'Insert', items: 'image link | charmap hr anchor pagebreak insertdatetime nonbreaking template' },
                    view: { title: 'View', items: 'visualchars visualblocks | preview fullscreen' },
                    format: { title: 'Format', items: 'bold italic underline strikethrough superscript subscript | formats | removeformat' },
                    table: { title: 'Table', items: 'inserttable tableprops deletetable cell row column' }
                },
                file_picker_types: 'image',
                file_picker_callback: function (cb, value, meta) {
                    // Create image file picker with a hidden input field.
                    var input = document.createElement('input');
                    input.setAttribute('type', 'file');
                    input.setAttribute('accept', 'image/*');

                    // Make the 'Source' textbox readonly so that the blobUri cannot be edited.
                    $('.mce-has-open .mce-textbox').attr('readonly', 'true');

                    input.onchange = function () {
                        var file = this.files[0];

                        var reader = new FileReader();
                        reader.readAsDataURL(file);
                        reader.onload = function () {
                            // Note: Now we need to register the blob in TinyMCEs image blob
                            // registry. In the next release this part hopefully won't be
                            // necessary, as they are looking to handle it internally.
                            var id = 'blobid' + (new Date()).getTime();
                            var blobCache = tinymce.activeEditor.editorUpload.blobCache;
                            var base64 = reader.result.split(',')[1];
                            var blobInfo = blobCache.create(id, file, base64);
                            blobCache.add(blobInfo);

                            // Call the callback and populate the alt field with the file name.
                            cb(blobInfo.blobUri(), { alt: file.name });
                        };
                    };

                    input.click();
                },
                setup: function (ed) {
                    ed.on('Change Redo Undo SetContent', function (evt) {
                        var form = $(this.getElement()).closest('form');
                        widget.setDirty(form.attr('id'));
                    });

                    ed.on('keyup', function (evt) {
                        var form = $(this.getElement()).closest('form');
                        widget.setDirty(form.attr('id'));
                    });

                    // set the location so that plug-ins load correctly
                    ed.editorManager.AddOnManager.baseURL = '/Scripts/tinymce';
                },
                // This does the stripping of rich text during a paste
                paste_preprocess: function (plugin, args) {
                    var encodedInitial = encodeURI(args.content);
                    var encodedFinal = encodedInitial.replace(/\+/g, '%2B'); // replace plus-sign (+) with its URL-encoding (2B)
                    var richTextEncoded = '{ "html" : "' + encodedFinal + '" }'; // must be URL-encoded for JSON

                    // call the server to do the scrub
                    $.ajax({
                        type: 'POST',
                        async: false, // call MUST be synchronous so args.content will be set before the function exits
                        url: '/default/GenBOE/PreProcessRichTextPaste',
                        contentType: 'application/json; charset=utf-8',
                        data: richTextEncoded,
                        dataType: 'html', // response is HTML
                        success: function (response) {
                            args.content = response;
                        },
                        error: function (response) {
                        },
                        complete: function (response) {
                        }
                    });
                },
                //Once it's initialized, and there's a refresh module/page function, then call it
                init_instance_callback: function () {
                    var mceContainer = $(this.getContainer());
                    
                    // BOEJ-2849 - clear the title (tooltip) attribute for the tinymce editor iframe.
                    mceContainer.find('iframe').attr('title', '');
                }
            };
        },

        /* 
            Makes a uibModal dialog draggable and resizable 
            Parameters:
                selector - JQuery selector used to find the dialog element in the DOM.
                           Note: uibModal wraps this element with an outer <div class="modal"> element. 
                minHeight - Minimum height when resizing the dialog.
                minWidth - Minimum width when resizing the dialog.
        */
        makeModalDraggableAndResizable: function (selector, minHeight, minWidth) {
            var $modal = $(selector).closest('.modal');
            $modal.find('.modal-content').resizable({
                minHeight: minHeight,
                minWidth: minWidth
            });
            $modal.find('.modal-dialog').draggable();
        }
    };

    return utilityService;

}]);

﻿using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Layouts.Elements;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Helpers;
using Orchard.Layouts.ViewModels;
using Orchard.MediaLibrary.Models;
using ContentItem = Orchard.Layouts.Elements.ContentItem;

namespace Orchard.Layouts.Drivers {
    public class ImageDriver : ElementDriver<Image> {
        private readonly IContentManager _contentManager;

        public ImageDriver(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        protected override EditorResult OnBuildEditor(Image element, ElementEditorContext context) {

            var viewModel = new ImageEditorViewModel();
            var editor = context.ShapeFactory.EditorTemplate(TemplateName: "Elements.Image", Model: viewModel);

            if (context.Updater != null) {
                context.Updater.TryUpdateModel(viewModel, context.Prefix, null, null);
                element.MediaId = ParseImageId(viewModel.ImageId);
            }

            var imageId = element.MediaId;
            viewModel.CurrentImage = imageId != null ? GetImage(imageId.Value) : default(ImagePart);

            return Editor(context, editor);
        }

        protected override void OnDisplaying(Image element, ElementDisplayContext context) {
            var imageId = element.MediaId;
            var image = imageId != null ? GetImage(imageId.Value) : default(ImagePart);
            context.ElementShape.ImagePart = image;
        }

        protected override void OnExporting(Image element, ExportElementContext context) {
            var image = element.MediaId != null ? GetImage(element.MediaId.Value) : default(ImagePart);

            if (image == null)
                return;

            context.ExportableState["Image"] = _contentManager.GetItemMetadata(image).Identity.ToString();
        }

        protected override void OnImporting(Image element, ImportElementContext context) {
            var imageIdentity = context.ExportableState.Get("Image");
            var image = !String.IsNullOrWhiteSpace(imageIdentity) ? context.Session.GetItemFromSession(imageIdentity) : default(ContentManagement.ContentItem);

            element.MediaId = image != null ? image.Id : default(int?);
        }

        protected ImagePart GetImage(int id) {
            return _contentManager.Get<ImagePart>(id, VersionOptions.Published);
        }

        private static int? ParseImageId(string imageId) {
            return ContentItem.Deserialize(imageId).FirstOrDefault();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Resources;

namespace ZumtenSoft.WebsiteCrawler.Resources
{
    /// <summary>
    /// Interface statique vers tous les icones utilisés dans l'application
    /// </summary>
    /// <example>
    /// En XAML: {x:Static Resources:BitmapIcons.Save}
    /// </example>
    public static class BitmapIcons
    {
        private static BitmapFrame BuildFrame(string path)
        {
            Uri iconUri = new Uri("pack://application:,,,/Resources/Icons/" + path, UriKind.RelativeOrAbsolute);
            return BitmapFrame.Create(iconUri);
        }

        private static BitmapImage BuildBitmap(string path)
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/" + path));
        }

        public static readonly BitmapFrame
            Application = BuildFrame("Application.ico");

        public static readonly BitmapImage
            New = BuildBitmap("Actions/New.png"),
            Add = BuildBitmap("Actions/Add.png"),
            Edit = BuildBitmap("Actions/Edit.png"),
            Delete = BuildBitmap("Actions/Delete.png"),
            Close = BuildBitmap("Actions/Close.png"),
            Open = BuildBitmap("Actions/Open.png"),
            Redo = BuildBitmap("Actions/Redo.png"),
            Undo = BuildBitmap("Actions/Undo.png"),
            Save = BuildBitmap("Actions/Save.png"),
            MoveUp = BuildBitmap("Actions/MoveUp.png"),
            MoveDown = BuildBitmap("Actions/MoveDown.png"),
            ControlPlay = BuildBitmap("Actions/ControlPlay.png"),
            ControlStop = BuildBitmap("Actions/ControlStop.png"),
            ControlPause = BuildBitmap("Actions/ControlPause.png"),

            FileExcel = BuildBitmap("Elements/FileExcel.png"),
            Folder = BuildBitmap("Elements/Folder.png"),
            DomainName = BuildBitmap("Elements/DomainName.png"),
            
            StatusWaiting = BuildBitmap("Status/Waiting.png"),
            StatusIgnored = BuildBitmap("Status/Ignored.gif"),
            StatusAccept = BuildBitmap("Status/Accept.png"),
            StatusError = BuildBitmap("Status/Error.png"),
            StatusProcessing = BuildBitmap("Status/Processing.gif"),
            StatusWarning = BuildBitmap("Status/Warning.png"),
            StatusWorking = BuildBitmap("Status/Working.png"),
            StatusRedirect = BuildBitmap("Status/Redirect.png");

        public static Dictionary<ResourceStatus, BitmapImage> WorkStatuses = new Dictionary<ResourceStatus, BitmapImage>
        {
            {ResourceStatus.ReadyToProcess, StatusWaiting},
            {ResourceStatus.Processing, StatusWorking},
            {ResourceStatus.Processed, StatusAccept},
            {ResourceStatus.Error, StatusError},
            {ResourceStatus.Timeout, StatusError},
            {ResourceStatus.Ignored, StatusIgnored},
        };

        public static BitmapImage GetImageFromHttpStatus(HttpStatusCode status)
        {
            int code = (int) status;
            if (code >= 200 && code < 300)
                return StatusAccept;
            if (code >= 300 && code < 400)
                return StatusRedirect;
            if (code >= 400)
                return StatusError;
            return null;
        }
    }

    /// <summary>
    /// Permet d'envelopper certains icones de BitmapImages à l'interieur du
    /// contrôle "Image". Utilisé pour simplifier le XAML pour les boutons de
    /// la toolbar et du menu.
    /// </summary>
    public static class ImageIcons
    {
        private static Image ToImage(this BitmapImage source)
        {
            return new Image { Source = source, Width = 16, Height = 16 };
        }

        public static Image New { get { return BitmapIcons.New.ToImage(); } }
        public static Image Open { get { return BitmapIcons.Open.ToImage(); } }
        public static Image Save { get { return BitmapIcons.Save.ToImage(); } }

        public static Image ControlPlay { get { return BitmapIcons.ControlPlay.ToImage(); } }
        public static Image ControlStop { get { return BitmapIcons.ControlStop.ToImage(); } }
        public static Image FileExcel { get { return BitmapIcons.FileExcel.ToImage(); } }
    }
}

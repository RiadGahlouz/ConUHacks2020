﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.IO;
using WPFMediaKit.DirectShow.Controls;
using MetriCam2.Cameras;
using System.Windows.Interop;
using System.Drawing;
using System.Timers;

namespace FaceEmotion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Add your Face subscription key to your environment variables.
        private const string subscriptionKey = "eb2e46c6e92c47bc80fc4c8c3f6cd9d3";
        // Add your Face endpoint to your environment variables.
        private const string faceEndpoint = "https://conuhacks2020.cognitiveservices.azure.com/";

        private readonly IFaceClient faceClient = new FaceClient(
            new ApiKeyServiceClientCredentials(subscriptionKey),
            new System.Net.Http.DelegatingHandler[] { });

        // The list of detected faces.
        private IList<DetectedFace> faceList;
        // The list of descriptions for the detected faces.
        private string[] faceDescriptions;
        // The resize factor for the displayed image.
        private double resizeFactor;

        private const string defaultStatusBarText =
            "Place the mouse pointer over a face to see the face description.";

        private WebCam webCam;

        private const string CAM_IMG_FILENAME = ".\\test.png";


        //public WriteableBitmap ImageWebcam
        //{
        //    get
        //    {
        //        return this.imageWebcam;
        //    }
        //    set
        //    {
        //        this.imageWebcam = value;
        //        this.NotifyOfPropertyChange(() => this.FacePhoto);
        //    }
        //}

        public MainWindow()
        {
            InitializeComponent();
            foreach (var n in MultimediaUtil.VideoInputNames)
            {
                Console.WriteLine(n);
            }

            //FacePhotos.VideoCaptureDevice = MultimediaUtil.VideoInputDevices[0];

            if (Uri.IsWellFormedUriString(faceEndpoint, UriKind.Absolute))
            {
                faceClient.Endpoint = faceEndpoint;
            }
            else
            {
                MessageBox.Show(faceEndpoint,
                    "Invalid URI", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 3500;
            aTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            FacePhoto.Dispatcher.Invoke(new UpdateTextCallback(this.BrowseButton_Click), new object[] { null, null });

            //BrowseButton_Click(null, null);
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var file = File.Open(CAM_IMG_FILENAME, FileMode.OpenOrCreate))
            {
                bitmap.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }

            MemoryStream ms = new MemoryStream();

            using(var file = File.OpenRead(CAM_IMG_FILENAME))
            {
                file.CopyTo(ms);
            }


            //BitmapImage bitmapSource = new BitmapImage(new Uri("D:\\Riad\\@Programming\\ConUHacks2020\\test.png"));
            BitmapImage bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.CacheOption = BitmapCacheOption.None;
            bitmapSource.StreamSource = ms;
            bitmapSource.EndInit();
            return bitmapSource;
        }
        public delegate void UpdateTextCallback(object sender, RoutedEventArgs e);
        // Displays the image and calls UploadAndDetectFaces.
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (webCam == null)
            {
                webCam = new WebCam();
                webCam.Connect();
            }

            //// Get the image file to scan from the user.
            //var openDlg = new Microsoft.Win32.OpenFileDialog();

            //openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            //bool? result = openDlg.ShowDialog(this);

            //// Return if canceled.
            //if (!(bool)result)
            //{
            //    return;
            //}

            //// Display the image file.
            //string filePath = openDlg.FileName;

            webCam.Update();
            var imageBase = webCam.CalcSelectedChannel();
            var bitmapSource = Bitmap2BitmapImage(imageBase.ToBitmap());


            //Uri fileUri = new Uri(filePath);
            //BitmapImage bitmapSource = new BitmapImage();

            //bitmapSource.BeginInit();
            //bitmapSource.CacheOption = BitmapCacheOption.None;
            //bitmapSource.UriSource = fileUri;
            //bitmapSource.EndInit();
            FacePhoto.Source = bitmapSource;

            // Detect any faces in the image.
            Title = "Detecting...";
            faceList = await UploadAndDetectFaces(null);
            File.Delete(CAM_IMG_FILENAME);

            Title = String.Format(
                "Detection Finished. {0} face(s) detected", faceList.Count);

            if (faceList.Count > 0)
            {
                // Prepare to draw rectangles around the faces.
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                // Some images don't contain dpi info.
                resizeFactor = (dpi == 0) ? 1 : 96 / dpi;
                faceDescriptions = new String[faceList.Count];

                for (int i = 0; i < faceList.Count; ++i)
                {
                    DetectedFace face = faceList[i];

                    // Draw a rectangle on the face.
                    drawingContext.DrawRectangle(
                        System.Windows.Media.Brushes.Transparent,
                        new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 2),
                        new Rect(
                            face.FaceRectangle.Left * resizeFactor,
                            face.FaceRectangle.Top * resizeFactor,
                            face.FaceRectangle.Width * resizeFactor,
                            face.FaceRectangle.Height * resizeFactor
                            )
                    );

                    // Store the face description.
                    faceDescriptions[i] = FaceDescription(face);
                }

                drawingContext.Close();

                // Display the image with the rectangle around the face.
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);

                faceWithRectBitmap.Render(visual);
                FacePhoto.Source = faceWithRectBitmap;

                // Set the status bar text.
                faceDescriptionStatusBar.Text = defaultStatusBarText;
            }

        }
        // Displays the face description when the mouse is over a face rectangle.
        private void FacePhoto_MouseMove(object sender, MouseEventArgs e)
        { // If the REST call has not completed, return.
            if (faceList == null)
                return;

            // Find the mouse position relative to the image.
            System.Windows.Point mouseXY = e.GetPosition(FacePhoto);

            ImageSource imageSource = FacePhoto.Source;
            BitmapSource bitmapSource = (BitmapSource)imageSource;

            // Scale adjustment between the actual size and displayed size.
            var scale = FacePhoto.ActualWidth / (bitmapSource.PixelWidth / resizeFactor);

            // Check if this mouse position is over a face rectangle.
            bool mouseOverFace = false;

            for (int i = 0; i < faceList.Count; ++i)
            {
                FaceRectangle fr = faceList[i].FaceRectangle;
                double left = fr.Left * scale;
                double top = fr.Top * scale;
                double width = fr.Width * scale;
                double height = fr.Height * scale;

                // Display the face description if the mouse is over this face rectangle.
                if (mouseXY.X >= left && mouseXY.X <= left + width &&
                    mouseXY.Y >= top && mouseXY.Y <= top + height)
                {
                    faceDescriptionStatusBar.Text = faceDescriptions[i];
                    mouseOverFace = true;
                    break;
                }
            }

            // String to display when the mouse is not over a face rectangle.
            if (!mouseOverFace) faceDescriptionStatusBar.Text = defaultStatusBarText;
        }

        // Uploads the image file and calls DetectWithStreamAsync.
        private async Task<IList<DetectedFace>> UploadAndDetectFaces(Stream imgSrc)
        {
            // The list of Face attributes to return.
            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
            FaceAttributeType.Gender, FaceAttributeType.Age,
            FaceAttributeType.Smile, FaceAttributeType.Emotion,
            FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            // Call the Face API.
            try
            {
                using (Stream imageFileStream = File.OpenRead(CAM_IMG_FILENAME))
                //using(Stream imageFileStream = imgSrc)
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    Console.WriteLine("Sending the image!");
                    IList<DetectedFace> faceList =
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);
                    Console.WriteLine("Got " + faceList.Count + " faces!");
                    return faceList;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException f)
            {
                MessageBox.Show(f.Message);
                return new List<DetectedFace>();
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
                return new List<DetectedFace>();
            }

        }

        // Creates a string out of the attributes describing the face.
        private string FaceDescription(DetectedFace face)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Face: ");

            // Add the gender, age, and smile.
            sb.Append(face.FaceAttributes.Gender);
            sb.Append(", ");
            sb.Append(face.FaceAttributes.Age);
            sb.Append(", ");
            sb.Append(String.Format("smile {0:F1}%, ", face.FaceAttributes.Smile * 100));

            // Add the emotions. Display all emotions over 10%.
            sb.Append("Emotion: ");
            Emotion emotionScores = face.FaceAttributes.Emotion;
            if (emotionScores.Anger >= 0.1f) sb.Append(
                String.Format("anger {0:F1}%, ", emotionScores.Anger * 100));
            if (emotionScores.Contempt >= 0.1f) sb.Append(
                String.Format("contempt {0:F1}%, ", emotionScores.Contempt * 100));
            if (emotionScores.Disgust >= 0.1f) sb.Append(
                String.Format("disgust {0:F1}%, ", emotionScores.Disgust * 100));
            if (emotionScores.Fear >= 0.1f) sb.Append(
                String.Format("fear {0:F1}%, ", emotionScores.Fear * 100));
            if (emotionScores.Happiness >= 0.1f) sb.Append(
                String.Format("happiness {0:F1}%, ", emotionScores.Happiness * 100));
            if (emotionScores.Neutral >= 0.1f) sb.Append(
                String.Format("neutral {0:F1}%, ", emotionScores.Neutral * 100));
            if (emotionScores.Sadness >= 0.1f) sb.Append(
                String.Format("sadness {0:F1}%, ", emotionScores.Sadness * 100));
            if (emotionScores.Surprise >= 0.1f) sb.Append(
                String.Format("surprise {0:F1}%, ", emotionScores.Surprise * 100));

            // Add glasses.
            sb.Append(face.FaceAttributes.Glasses);
            sb.Append(", ");

            // Add hair.
            sb.Append("Hair: ");

            // Display baldness confidence if over 1%.
            if (face.FaceAttributes.Hair.Bald >= 0.01f)
                sb.Append(String.Format("bald {0:F1}% ", face.FaceAttributes.Hair.Bald * 100));

            // Display all hair color attributes over 10%.
            IList<HairColor> hairColors = face.FaceAttributes.Hair.HairColor;
            foreach (HairColor hairColor in hairColors)
            {
                if (hairColor.Confidence >= 0.1f)
                {
                    sb.Append(hairColor.Color.ToString());
                    sb.Append(String.Format(" {0:F1}% ", hairColor.Confidence * 100));
                }
            }

            // Return the built string.
            return sb.ToString();
        }

    }
}

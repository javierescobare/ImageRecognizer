using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Uri = Android.Net.Uri;

namespace ImageRecognizer.Droid
{
    [Activity(Label = "ResultActivity")]
    public class ResultActivity : Activity
    {
        private VisionServiceClient visionClient;
        private const string SUBSCRIPTION_KEY = "67c186141e2244ea80b89411cae00854";
        private Uri fileUri;
        private ImageView imgPreview;
        private TextView description;

        protected async override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Result);

            visionClient = new VisionServiceClient(SUBSCRIPTION_KEY);
            imgPreview = FindViewById<ImageView>(Resource.Id.imageResult);
            description = FindViewById<TextView>(Resource.Id.textoResultado);

            //  Get File Uri from taken picture
            fileUri = (Uri)Intent.Extras.GetParcelable("fileUri");

            ShowPicture();

            ProgressDialog dialog = ProgressDialog.Show(this,
                        "Dame unos segundos!",
                        "Estoy analizando tu imagen");

            //  Analyze image
            ResizeImage(fileUri.Path, 800, 800);
            var result = await DescribeImage(new FileStream(fileUri.Path, FileMode.Open));
            dialog.Dismiss();

            //  Show answer
            if (result != null)
                description.Text = result.Description.Captions.First().Text;
            else
                description.Text = "Disculpa, no pude reconocer tu imagen :(.";
        }

        //  From StackOverFlow:
        //  https://forums.xamarin.com/discussion/comment/158633/#Comment_158633
        public void ResizeImage(string sourceFile, float maxWidth, float maxHeight)
        {
            if (File.Exists(sourceFile))
            {
                var options = new BitmapFactory.Options()
                {
                    InJustDecodeBounds = false,
                    InPurgeable = true,
                };

                using (var image = BitmapFactory.DecodeFile(sourceFile, options))
                {
                    if (image != null)
                    {
                        var sourceSize = new Size(
                            (int)image.GetBitmapInfo().Height,
                            (int)image.GetBitmapInfo().Width);

                        var maxResizeFactor = Math.Min(
                            maxWidth
                            / sourceSize.Width, maxHeight
                            / sourceSize.Height);

                        if (maxResizeFactor > 0.9)
                        {
                            File.Create(sourceFile);
                        }
                        else
                        {
                            var width = (int)(maxResizeFactor * sourceSize.Width);
                            var height = (int)(maxResizeFactor * sourceSize.Height);

                            using (var bitmapScaled = Bitmap.CreateScaledBitmap(image, height, width, true))
                            {
                                using (Stream outStream = File.Create(sourceFile))
                                {
                                    if (sourceFile.ToLower().EndsWith("png"))
                                        bitmapScaled.Compress(Bitmap.CompressFormat.Png, 100, outStream);
                                    else
                                        bitmapScaled.Compress(Bitmap.CompressFormat.Jpeg, 95, outStream);
                                }
                                bitmapScaled.Recycle();
                            }
                        }

                        //  Notify Android gallery to show the image
                        Android.Media.MediaScannerConnection.ScanFile(
                            this,
                            new string[] { sourceFile },
                            new string[] { "image/jpeg" },
                            null);

                        image.Recycle();
                    }
                }
            }
        }

        public async Task<AnalysisResult> DescribeImage(Stream imageStream)
        {
            try
            {
                return await visionClient.DescribeAsync(imageStream);
            }
            catch (Exception e)
            {
                Log.WriteLine(LogPriority.Error, "Error", e.Message);
                return null;
            }
        }

        private void ShowPicture()
        {
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InSampleSize = 8;
            Bitmap bitmap = BitmapFactory.DecodeFile(fileUri.Path, options);
            imgPreview.SetImageBitmap(bitmap);
        }
    }
}
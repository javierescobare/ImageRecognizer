using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Widget;
using System;
using System.IO;
using Environment = Android.OS.Environment;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace ImageRecognizer.Droid
{
    [Activity(Label = "CameraDemo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const int CAMERA_CAPTURE_IMAGE_REQUEST_CODE = 100;
        private const string IMAGE_DIRECTORY_NAME = "Reconocimiento de imágenes";
        private Uri fileUri;

        private Button btnCapturePicture;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            btnCapturePicture = FindViewById<Button>(Resource.Id.btnCapturePicture);

            btnCapturePicture.Click += (s, args) =>
            {
                TakePicture();
            };
        }

        #region HelperMethods

        private void TakePicture()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            fileUri = GetOutputMediaFile(
                IMAGE_DIRECTORY_NAME,
                String.Empty);
            intent.PutExtra(MediaStore.ExtraOutput, fileUri);
            StartActivityForResult(intent, CAMERA_CAPTURE_IMAGE_REQUEST_CODE);
        }

        //  From Github:
        //  https://github.com/xamurais/Xamarin.android/tree/master/CamaraFoto
        private Uri GetOutputMediaFile(string subdir, string name)
        {
            subdir = subdir ?? String.Empty;

            //  Name the pic
            if (String.IsNullOrWhiteSpace(name))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                name = "IMG_" + timestamp + ".jpg";
            }

            //  Get cellphone pictures directory
            string mediaType = Environment.DirectoryPictures;

            //  Get complete path
            using (Java.IO.File mediaStorageDir = new Java.IO.File(
                Environment.GetExternalStoragePublicDirectory(mediaType), subdir))
            {
                //  If the directory doesn't exist, create it
                if (!mediaStorageDir.Exists())
                {
                    if (!mediaStorageDir.Mkdirs())
                        throw new IOException("No se pudo crear el directorio");
                }

                return Uri.FromFile(new Java.IO.File(GetUniquePath(mediaStorageDir.Path, name)));
            }
        }

        private string GetUniquePath(string path, string name)
        {
            //  Apply a unique name
            string ext = Path.GetExtension(name);
            if (ext == String.Empty)
                ext = ".jpg";

            name = Path.GetFileNameWithoutExtension(name);

            string nname = name + ext;
            int i = 1;
            while (File.Exists(Path.Combine(path, nname)))
                nname = $"{name}_{i++}{ext}";

            return Path.Combine(path, nname);
        }

        #endregion HelperMethods

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == CAMERA_CAPTURE_IMAGE_REQUEST_CODE)
            {
                //  Check answer (OK or canceled)
                if (resultCode == Result.Ok)
                {
                    Intent i = new Intent(this, typeof(ResultActivity));
                    i.PutExtra("fileUri", fileUri);
                    StartActivity(i);
                }
                else if (resultCode == Result.Canceled)
                {
                    Toast.MakeText(this.ApplicationContext,
                        "La captura de la imagen fue cancelada.",
                        ToastLength.Short)
                        .Show();
                }
                else
                {
                    Toast.MakeText(this.ApplicationContext,
                        "Ups! Algo raro pasó. Inténtalo de nuevo :)!",
                        ToastLength.Short)
                        .Show();
                }
            }
        }
    }
}

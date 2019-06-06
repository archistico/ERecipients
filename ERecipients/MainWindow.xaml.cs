using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace ERecipients
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string testo;

        public static byte[] ConvertiByte(string tx)
        {
            //byte[] bytes= System.Text.Encoding.UTF8.GetBytes(tx);
            byte[] bytes = System.Text.Encoding.Unicode.GetBytes(tx);
            return bytes;
        }

        public void ExportToPng(string path, Canvas surface)
        {
            if (path == null) return;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(surface.Width, surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        public static void SaveCanvas(Window window, Canvas canvas, int dpi, string filename)
        {
            Size size = new Size(window.Width, window.Height);
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));

            var rtb = new RenderTargetBitmap(
                (int)window.Width, //width
                (int)window.Height, //height
                dpi, //dpi x
                dpi, //dpi y
                PixelFormats.Pbgra32 // pixelformat
                );
            rtb.Render(canvas);

            SaveRTBAsPNG(rtb, filename);
        }

        private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
        {
            var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
            enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

            using (var stm = System.IO.File.Create(filename))
            {
                enc.Save(stm);
            }
        }

        private void btConvert_Click(object sender, RoutedEventArgs e)
        {
            string testoDaTagliare = tbText.Text;

            List<string> Lista = new List<string>();
            // Divido in stringhe di testo di 8 caratteri

            while (testoDaTagliare.Length > 8)
            {
                Lista.Add(testoDaTagliare.Substring(0, 8));
                testoDaTagliare = testoDaTagliare.Substring(8, testoDaTagliare.Length - 8);
            }

            Lista.Add(testoDaTagliare.Substring(0, testoDaTagliare.Length));

            int contFile = 0;

            foreach (string tx in Lista)
            {

                //Converto nel sistema di codifica desiderato e in un array di byte 
                byte[] bytes = ConvertiByte(tx);
                //Converto l'array di byte in bits
                BitArray bits = new BitArray(bytes);

                //MessageBox.Show("Array di byte lungo : " + bytes.Length);
                //MessageBox.Show("Array di bit lungo : " + bits.Length);

                int dvX = 50;
                int dvY = 50;

                int origineX = 0;
                int origineY = 0;

                int puntoX = 0;
                int puntoY = 0;

                int dim = 25;
                int contB = 0;

                //List<System.Windows.Shapes.Rectangle> ListRect = new List<System.Windows.Shapes.Rectangle>();

                // VERIFICA CODICE BINARIO CREATO
                Debug.WriteLine("-------------------");
                for (int c = 0; c < bits.Length; c++)
                {
                    if (c % 4 == 0 && c != 0)
                        Debug.Write(" ");
                    if (bits[c])
                        Debug.Write("1");
                    else
                        Debug.Write("0");
                }
                Debug.WriteLine("\n-------------------");

                // CREA LE RIGHE
                for (int br = 0; br <= 2; br++)
                {
                    puntoX = 0;
                    //CREA LE COLONNE
                    for (int bc = 0; bc <= 2; bc++)
                    {
                        // RIGHE DEL BLOCCO
                        for (int cr = 0; cr <= 3; cr++)
                        {
                            // COLONNE DEL BLOCCO
                            for (int cc = 0; cc <= 3; cc++)
                            {

                                // SALTO SE BLOCCO CENTRALE
                                if (br == 1 && bc == 1)
                                {
                                    puntoX = puntoX + 100;
                                    bc++;
                                }

                                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                                rect.Stroke = new SolidColorBrush(Colors.Black);

                                if (bits.Length < contB + 1)
                                    rect.Fill = new SolidColorBrush(Colors.Red);
                                else
                                {
                                    if (bits[contB++])
                                        rect.Fill = new SolidColorBrush(Colors.Black);
                                    else
                                        rect.Fill = new SolidColorBrush(Colors.White);
                                }
                                rect.Width = dim;
                                rect.Height = dim;

                                Canvas.SetLeft(rect, dvX + puntoX + origineX + cc * dim);
                                Canvas.SetTop(rect, dvY + puntoY + origineY + cr * dim);

                                // ListRect.Add(rect);
                                Immagine.Children.Add(rect);

                            } // FINE FOR CC
                        } // FINE FOR CR

                        puntoX = puntoX + dim * 4;
                    } // FINE FOR BC

                    puntoY = puntoY + dim * 4;
                }// FINE FOR BR


                // DISEGNO RETTANGOLO CENTRALE
                
                puntoX = 100;
                puntoY = 100;
                int contCentrali = 0;
                //                                  1     2     3     4     5     6      7     8     9     10     11     12    13    14    15    16
                bool[] bitCentrali = new bool[16] { true, true, true, true, true, false, true, true, true, false, false, true, true, true, true, true };

                // RIGHE DEL BLOCCO
                for (int cr = 0; cr <= 3; cr++)
                {
                    // COLONNE DEL BLOCCO
                    for (int cc = 0; cc <= 3; cc++)
                    {
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                        rect.Stroke = new SolidColorBrush(Colors.Black);
                                                
                            if (bitCentrali[contCentrali++])
                                rect.Fill = new SolidColorBrush(Colors.Black);
                            else
                                rect.Fill = new SolidColorBrush(Colors.White);
                        
                        rect.Width = dim;
                        rect.Height = dim;

                        Canvas.SetLeft(rect, dvX + puntoX + origineX + cc * dim);
                        Canvas.SetTop(rect, dvY + puntoY + origineY + cr * dim);

                        // ListRect.Add(rect);
                        Immagine.Children.Add(rect);

                    } // FINE FOR CC
                } // FINE FOR CR

                SaveCanvas(this, this.Immagine, 96, "file" + contFile++ + ".png");
            } // FINE FOREACH
        }

    }
}

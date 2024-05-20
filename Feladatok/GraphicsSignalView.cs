using Signals.DocView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Signals
{
    public partial class GraphicsSignalView : UserControl, IView
    {
        SignalDocument document;

        double zoom = 1;
        public int ViewNumber { get; set; }

        public GraphicsSignalView()
        {
            InitializeComponent();
        }

        public GraphicsSignalView(SignalDocument document)
    : this()
        {
            this.document = document;
        }

        public new void Update()
        {
            Invalidate();
        }

        public Document GetDocument()
        {
            return document;
        }

        /// <summary>
        /// A UserControl.Paint felüldefiniálása, ebben rajzolunk.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //toll létrehozása
            var pen = new Pen(Color.Blue, 2)
            {
                DashStyle = DashStyle.Dot,
                EndCap = LineCap.ArrowAnchor,
            };

            //y tengely kirajzolása az ablak teljes magasságában (height-tól 0-ig), az ablak bal szélén (0 helyett 2-ben, hogy látsszon a vonal)
            e.Graphics.DrawLine(pen, 2, ClientSize.Height, 2, 0);

            //x tengely kirajzolása az ablak teljes széltében (2-től width-ig) víízszintesen pedig középen (a magasság fele)
            e.Graphics.DrawLine(pen, 2, ClientSize.Height / 2, ClientSize.Width, ClientSize.Height / 2);



            //a jeleket elmentjük a data tömbbe
            var data = document.Signals;

            // Ha nincs adat, visszatér rajzolás nélkül
            if (data.Count == 0)
                return;

            // Időrendbe rakjuk a jeleket, hogy ne ugráljon a diagram az x tengelyen. A beépített OrderBy függvény a kapott értékek szerint rendez sorba, ez ebben az esetben a data minden elemének időpontja
            var list = data.OrderBy(x => x.getTimeStamp());
            //az első adat az x=0-ban kezdődik
            float x = 0;
            //Az eredeti értéket egy negatív értékkel szorzom, és eltolom még függőlegesen középre a height/2 - vel, így lesz kicsivel az x tengely felett és arányosan a többihez (nem csináltam külön pixelPerValue változót hozzá) 
            float y = ((float)list.First().getValue() * -15 * (float)zoom + ClientSize.Height / 2.0f);

            //elmentem a koordinátákat, hogy később ezek segítségével rajzoljam ki a vonalat, és számoljam az x-et. Az időt is eltárolom az x számolásához
            float lastX = x;
            float lastY = y;
            DateTime lastTime = list.First().getTimeStamp();

            //a ciklusban az első elem koordinátái már megvannak adva, ezért azt csak kirajzolom. Ezután az x értéket úgy adom, hogy a jelenlegi időből kivonom az előzőt és hozzáadom az előző x értéket.
            //A nagy számmal való osztás a Ticks óriási értéke miatt kell, így közelítjük  a képernyő méreteihez. Az y-t ugyanúgy számoljuk, mint a ciklus előtt. Ezen kívül 
            foreach (var item in list)
            {
                if (item == list.First()) e.Graphics.FillRectangle(Brushes.Blue, x, y, 3, 3);
                else
                {
                    x = ((item.getTimeStamp().Ticks - lastTime.Ticks) / 1000000000000 + lastX) * (float)zoom;
                    y = ((float)item.getValue() * -15 * (float)zoom + ClientSize.Height / 2);
                    //kirajzoljuk a pontot és a vonalat az előző koordináta és a mostani közt.
                    e.Graphics.FillRectangle(Brushes.Blue, x, y, 3, 3);
                    e.Graphics.DrawLine(Pens.Blue, x, y, lastX, lastY);
                    lastX = x;
                    lastY = y;
                    lastTime = item.getTimeStamp();
                }
            }



        }

        private void button2_Click(object sender, EventArgs e)
        {
            zoom *= 1.2;
            Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            zoom /= 1.2;
            Invalidate();
        }
    }
}

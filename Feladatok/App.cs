using Signals.DocView;

namespace Signals
{
    /// <summary>
    /// Az alkalmazást reprezentálja. Egy példányt kell létrehozni belőle az Initialize
    /// hívásával, ez lesz a "gyökérobjektum". Ez bármely osztály számára hozzáférhető az
    /// App.Instance propertyn keresztül.
    /// </summary>
    public class App
    {
        /// <summary>
        /// Az alkalmazásobjektum tárolására szolgál.
        /// </summary>
        private static App theApp = new App();

        /// <summary>
        /// Elérhetővé teszi mindenki számára az alkalmazásobjektumot (App.Instance-ként
        /// érhető el.)
        /// </summary>
        public static App Instance
        {
            get { return theApp; }
        }

        /// <summary>
        /// Az aktív nézet (melynek tabfüle ki van választva).
        /// </summary>
        private IView activeView;

        /// <summary>
        /// Az alkalmazáshoz tartozó dokumentumok listája.
        /// </summary>
        private List<Document> documents = new List<Document>();

        /// <summary>
        /// Összeköti az alkalmazást a főablakkal.
        /// </summary>
        public static void Initialize(MainForm form)
        {
            theApp.MainForm = form;
        }

        /// <summary>
        /// A főablak.
        /// </summary>
        public MainForm MainForm { get; private set; }

        /// <summary>
        /// Visszaadja az aktív dokumentumot.
        /// </summary>
        public Document ActiveDocument
        {
            get
            {
                if (activeView == null)
                    return null;

                return activeView.GetDocument();
            }
        }

        /// <summary>
        /// Létrehoz egy új dokumentumot a hozzá tartozó nézettel.
        /// </summary>
        public void NewDocument()
        {
            // Bekérdezzük az új font típus (dokumentum) nevét a felhasználótól egy modális dialógs ablakban.
            var form = new NewDocForm();
            if (form.ShowDialog() != DialogResult.OK)
                return;

            // Új dokumentum objektum létrehozása és felvétele a dokumentum listába.
         
            var doc = new SignalDocument(form.DocName);
            documents.Add(doc);

            CreateView(doc, true);
        }

        /// <summary>
        /// Frissíti az activeDocument változót, hogy az aktuálisan kiválasztott tabhoz tartozó dokumentumra
        /// mutasson.
        /// </summary>
        public void UpdateActiveView()
        {
            activeView = MainForm.TabControl.TabPages.Count != 0
                ? (IView)MainForm.TabControl.SelectedTab.Tag
                : null;
        }

        /// <summary>
        /// Megnyit egy dokumentumot. Nincs implementálva.
        /// </summary>
        public void OpenDocument()
        {
            // 1. Fájl útvonal megkérdezése a felhasználótól (OpenFileDialog).
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.openfiledialog
            // Lényeges, hogy az dokumentációban látható példával ellentétben NE nyissuk meg a fájlt
            // a dialóguson OpenFile hívásával, helyette egyszerűen a FileName property
            // segítségével kérdezzük le a felhasználó által megadott útvonalat
            // (amennyiben a felhasználó az OK-kal zárta be az ablakot).
            string filePath = string.Empty;
            
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
               
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    
                    filePath = openFileDialog.FileName;
                }
            }

            // 2. Új dokumentum objektum létrehozása, regisztrálása, nézet létrehozása, stb.
            // A NewDocument művelet szolgálhat mintaként.
            // A dokumentum neve a fájl neve legyen a könyvtár nélkül (ehhez használja a
            // System.IO.Path osztály GetFileName statikus függvényét)
            SignalDocument sd = new SignalDocument(Path.GetFileName(filePath));
            documents.Add(sd);
            CreateView(sd, true);

            // 3. Dokumentumba adatok betöltése (LoadDocument hívása  dokumentum objektumon)
            sd.LoadDocument(filePath);

            // 4. Nézetek értesítése, hogy frissítsék magukat.
            // Megjegyzés. Erre alapvetően a dokumentum osztály UpdateAllViews művelete használandó.
            // Ez viszont  protected , itt nem elérhető. Ne  tegye publikussá! Helyette kövesse
            // az egységbezárás alapelvét: az előző, 3. lépésben meghívta a SignalDocument.LoadDocument
            // műveletét. Tegye az UpdateAllViews hívást a LoadDocument végére. A Document-View
            // architektúrának ez az egyik lényegi pontja: minden dokumentum módosító művelet végére
            // oda kell tenni az UpdateAllViews hívást, hogy a nézetek tükrözzék a változásokat.
        }

        /// <summary>
        /// Elmenti az aktív dokumentum tartalmát. Nincs implementálva.
        /// </summary>
        public void SaveActiveDocument()
        {
            if (ActiveDocument == null)
                return;

            // Útvonal bekérése a felhasználótól a SaveFileDialog segítségével.
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.savefiledialog
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            sfd.FilterIndex = 0;
            sfd.RestoreDirectory = true;
            

            if(sfd.ShowDialog() != DialogResult.OK) return;

            // A dokumentum adatainak elmentése.
            ActiveDocument.SaveDocument(sfd.FileName);
        }

        /// <summary>
        /// Bezárja az aktív dokumentumot.
        /// </summary>
        public void CloseActiveView()
        {
            if (MainForm.TabControl.TabPages.Count == 0)
                return;

            var docToClose = ActiveDocument;

            // Eltávolítjuk a nézetet a dokumentum nézet listájából
            docToClose.DetachView(activeView);

            // Bezárjuk a view szülő tabját
            MainForm.TabControl.TabPages.Remove(GetTabPageForView(activeView));

            // Ha ez volt a dokumentum utolsó nézete, akkor a dokumentumot is bezárjuk,
            // eltávolítjuk a documents listából.
            if (!docToClose.HasAnyView)
                documents.Remove(docToClose);
        }

        /// <summary>
        /// Létrehoz egy új nézetet az aktív dokumentumhoz.
        /// </summary>
        public void CreateViewForActiveDocument()
        {
            if (ActiveDocument == null)
                return;

            CreateView(ActiveDocument, true);
        }

        /// <summary>
        /// Létrehoz egy új nézetet dokumentumhoz, és ezt be is regisztrálja a
        /// dokumentumnál (hogy a jövőben étesüljön a változásairól)
        /// Egy új tabfület is létrehoz a nézetnek.
        /// </summary>
        private IView CreateView(Document document, bool activateView)
        {
            // Új tab felvétele: az első paraméter egy kulcs, a második a tab felirata
            //mainForm.TabControl.TabPages.Add(form.DocName, form.DocName);
            var tp = new TabPage(document.Name);
            MainForm.TabControl.TabPages.Add(tp);

            var view = new GraphicsSignalView((SignalDocument)document);
            //TabPage tp = mainForm.TabControl.TabPages[form.DocName];
            tp.Controls.Add(view);
            tp.Tag = view;
            view.Dock = DockStyle.Fill;

            // A View beregisztrálása a dokumentumnál, hogy értesüljön majd a dokumentum változásairól.
            document.AttachView(view);

            // Ha az új nézet nem a dokumentum első nézete,
            // akkor a tabfülön a nézet sorszámát is megjelenítjük.
            if (view.ViewNumber > 1)
                tp.Text = tp.Text + ":" + view.ViewNumber;

            // Az új tab legyen a kiválasztott.
            if (activateView)
            {
                // Ennek hatására elsül a tab SelectedIndexChanged eseménykezelője,
                // ami meg beállítja az activeView tagváltozót
                MainForm.TabControl.SelectTab(tp);
                activeView = view;
            }

            return view;
        }

        /// <summary>
        /// Visszaadja az adott nézetet tartalmazó TabPage vezérlőt.
        /// Exception-t dob, ha nem találja.
        /// </summary>
        private TabPage GetTabPageForView(IView view)
        {
            foreach (TabPage page in MainForm.TabControl.TabPages)
                if (page.Tag == activeView)
                    return page;

            throw new Exception("Page for view not found.");
        }
    }
}

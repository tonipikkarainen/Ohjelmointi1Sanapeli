using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
using System.IO;

public class SaastaSilta : PhysicsGame
{
    private int n = 0;
    private int m = 0;
    private string[] kirjaimet =
        {"QWERTYUIOPÅ","ASDFGHJKLÖÄ","ZXCVBNM" };
    private string[] arvattavatSanat = LueTiedosto("sanat.txt");
    private string[] kuvat = LueTiedosto("kuvat.txt");
    private string sana;
    private int virheraja = 7;
    private int sillanKoko = 520;

    /// <summary>
    /// Luetaan tiedosto ja palautetaan tiedoston rivit.
    /// </summary>
    /// <param name="tiedosto">Luettava tiedosto.</param>
    /// <returns></returns>
    public static string[] LueTiedosto(string tiedosto)
    {
        try
        {
            return File.ReadAllLines(tiedosto); 
        }
        catch (IOException)
        {
            return null;
        }
    }
    Image taustaKuva = LoadImage("tausta");


    public override void Begin()
    {

        ClearGameObjects();
        ClearControls();
        //IsFullScreen = true;
        n = 0;
        m = 0;
        int arvottuLuku = RandomGen.NextInt(arvattavatSanat.Length);
        //Camera.ZoomToAllObjects(10);

        sana = arvattavatSanat[arvottuLuku];
        PhysicsObject kuva = Laatikko(new Vector(-300, 100), 300);
        kuva.Image = LoadImage(kuvat[arvottuLuku]);
        this.Add(kuva);

        Level.Background.Image = taustaKuva;
        Level.Background.ScaleToLevelFull();
        Mouse.IsCursorVisible = true;
        Vector paikka = new Vector(-620, -80);
        Gravity = new Vector(0, -200);
               
        PhysicsObject[] arvattava = ArvattavaSana(this,new Vector (-600,300), sana, 400);
        
        PhysicsObject[] silta = Silta(this, new Vector(150, 50), sillanKoko);
        
        Aakkoset(this, paikka, kirjaimet, 700, arvattava, silta[4]);

        Camera.ZoomToLevel();
        Keyboard.ListenAll(ButtonState.Pressed, VertaaPainettuKirjain, arvattava, silta[4]);
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä avustus");
        Keyboard.Listen(Key.F5, ButtonState.Pressed, Begin, "New game");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");       
    }
    

    /// <summary>
    /// Luo neliönmuotoisen laatikon.
    /// </summary>
    /// <param name="paikka">Laatikon paikkavektori</param>
    /// <param name="r">Neliön sivun pituus</param>
    /// <returns>Laatikko</returns>
    public static PhysicsObject Laatikko(Vector paikka, double r)
    {
        PhysicsObject kirjain = new PhysicsObject(r, r);
        kirjain.Position = paikka;
        kirjain.Image = LoadImage("tiilikuva");
        kirjain.IgnoresGravity=true;
        kirjain.IgnoresCollisionResponse = true;
        return kirjain;
    }


    /// <summary>
    /// Luodaan laatikko.
    /// </summary>
    /// <param name="paikka">Laatikon paikkavektori</param>
    /// <param name="korkeus">Laatikon korkeus</param>
    /// <param name="leveys">Laatikon leveys</param>
    /// <returns>Laatikko</returns>
    public static PhysicsObject Sillanosa(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject laatikko = new PhysicsObject(leveys, korkeus);
        laatikko.Position = paikka;
        laatikko.Image = LoadImage("tiilikuva");
        laatikko.IgnoresCollisionResponse = false;
        return laatikko;
    }


    /// <summary>
    /// Luo aakkoset.
    /// </summary>
    /// <param name="peli">Peli johon aakkoset lisätään.</param>
    /// <param name="paikka">Ensimmäisen kirjaimen paikka.</param>
    /// <param name="kirjaimet">Kirjaimet jotka lisätään.</param>
    /// <param name="leveys">Aakkosten leveys.</param>
    /// <param name="arvaus">Pelissä oleva arvattava sana.</param>
    /// <param name="sillanosa">Hävitessä tuhottava sillanosa.</param>
    public void Aakkoset(Game peli, Vector paikka, string[] kirjaimet, double leveys,
        PhysicsObject[] arvaus,  PhysicsObject sillanosa)
    {
        int ekanRivinKirjaimet = kirjaimet[0].Length;
        if (ekanRivinKirjaimet == 0) return;
        double koko = leveys / ekanRivinKirjaimet;
        Vector xsiirto = new Vector(1.5*koko, 0);
        Vector ysiirto = new Vector(0.3*koko, -koko*1.5);
        for (int rivi = 0; rivi < kirjaimet.Length; rivi++)
        {
            Vector kirjainpaikka = paikka + ysiirto * rivi;
            string jono = kirjaimet[rivi];
            for (int sarake = 0; sarake < kirjaimet[rivi].Length; sarake++)
            {
                char merkki = jono[sarake];
                PhysicsObject kirjain = Laatikko(kirjainpaikka , koko);
                MerkinLisays(merkki, kirjain);
                peli.Add(kirjain);
                Mouse.ListenOn(kirjain, MouseButton.Left,
                 ButtonState.Released,Vertaa, null, merkki, arvaus, sillanosa);
                kirjainpaikka += xsiirto;
            }
        }
    }


    /// <summary>
    /// Luo laatikot arvattavan sanan kirjaimille.
    /// </summary>
    /// <param name="paikka">Laatikoiden paikkavektori.</param>
    /// <param name="sana">Sana, joka laatikoihin täytyy sopia.</param>
    /// <param name="leveys">Laatikoiden kokonaisleveys.</param>
    /// <returns>Paikka arvattavalle sanalle</returns>
    public static PhysicsObject[] ArvattavaSana(Game peli, Vector paikka, string sana, double leveys)
    {
        int kirjaimet = sana.Length;
        //if (kirjaimet == 0) return;
        double koko = leveys / kirjaimet;
        PhysicsObject[] kirjainTaulukko =new PhysicsObject[sana.Length];
        Vector xsiirto = new Vector(1.5 * koko, 0);
        for (int sarake = 0; sarake < kirjaimet; sarake++)
        {
            char merkki = sana[sarake];
            kirjainTaulukko[sarake] = Laatikko(paikka,koko);
            peli.Add(kirjainTaulukko[sarake]);
            paikka += xsiirto;
        }

        return kirjainTaulukko;
    }

   
    /// <summary>
    /// Klittaessa väärää kirjainta pudottaa sillalle painoa.
    /// Kun painoa on pudotettu tarpeeksi tuhotaan silta.
    /// </summary>
    /// <param name="sillanosa">Tuhottava sillanosa.</param>
    public void Pudotus(PhysicsObject sillanosa)
    {
        PhysicsObject pudotettavat = Laatikko(new Vector(370, 350), 40);
        pudotettavat.Image = LoadImage("pulukuva");
        pudotettavat.IgnoresCollisionResponse = false;
        pudotettavat.IgnoresGravity = false;
        this.Add(pudotettavat);
        //m++;
        if(m==virheraja)
        {
            Explosion rajahdys = new Explosion(sillanosa.Width);
            rajahdys.Position = sillanosa.Position;
            rajahdys.UseShockWave = false;
            this.Add(rajahdys);
            sillanosa.Destroy();
            Lopetus("Hävisit :(");
        }
    }


    /// <summary>
    /// Lisätään merkki laatikkoon.
    /// </summary>
    /// <param name="k">Lisättävä merkki.</param>
    /// <param name="laatikko">Fysiikkaobjekti, johon merkki lisätään.</param>
    public void MerkinLisays(char k, PhysicsObject laatikko)
    {
        Label label = new Label("" + k);
        Font fontti = LoadFont("TestiFontti");
        label.Font = fontti;
        //label.Font = Font.DefaultLargeBold;
        laatikko.Add(label);
    }


    /// <summary>
    /// Uusi peli tai lopetus.
    /// </summary>
    /// <param name="viesti">Viesti, joka näytetään pelin loppuessa.</param>
    public void Lopetus(string viesti)
    {
        MultiSelectWindow valikko = new MultiSelectWindow(viesti,
        "Uusi peli", "Lopeta");
        valikko.ItemSelected += PainettiinValikonNappia;
        Add(valikko);
    }


    /// <summary>
    /// Riippuen valinnasta aloittaa uuden pelin tai lopettaa pelaamisen.
    /// </summary>
    /// <param name="valinta">Klikattu valinta.</param>
    void PainettiinValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                Begin();
                break;
            case 1:
                Exit();
                break;
        }
    }


    /// <summary>
    /// Luo sillan peliin.
    /// </summary>
    /// <param name="paikka">Sillan paikkavektori.</param>
    /// <returns>Silta</returns>
    public static PhysicsObject[] Silta(Game peli,Vector paikka, double koko)
    {

        double leveysJalka = koko / 24;
        double korkeusJalka = koko / 8;

        Vector xsiirto1 = new Vector(koko, 0);
        Vector tiputettavasiirto = new Vector(koko / 2, korkeusJalka*2/5);
        Vector palkki1siirto = new Vector(koko / 4, korkeusJalka*3/5);
        Vector palkki2siirto = new Vector(koko * 3/4, korkeusJalka * 3 / 5);
        PhysicsObject[] silta = new PhysicsObject[5];

        PhysicsObject vasenJalka = Sillanosa(paikka, leveysJalka, korkeusJalka);
        vasenJalka.MakeStatic();

        PhysicsObject oikeaJalka = Sillanosa(paikka + xsiirto1, leveysJalka, korkeusJalka);
        oikeaJalka.MakeStatic();

        PhysicsObject tiputettava = Sillanosa(paikka + tiputettavasiirto, koko - leveysJalka, korkeusJalka / 5);
        tiputettava.MakeStatic();

        PhysicsObject palkki1 = Sillanosa(paikka + palkki1siirto, koko / 2, korkeusJalka / 5);

        PhysicsObject palkki2 = Sillanosa(paikka + palkki2siirto, koko / 2, korkeusJalka / 5);

        silta[0] = vasenJalka;
        silta[1] = oikeaJalka;
        silta[2] = palkki1;
        silta[3] = palkki2;
        silta[4] = tiputettava;
        for (int i = 0; i < silta.Length; i++)
        {
            peli.Add(silta[i]);
        }
        return silta;
    }


    /// <summary>
    /// Vertaa klikattua kirjainta ja vuorossa olevaa arvattavan
    /// sanan kirjainta.
    /// </summary>
    /// <param name="merkki">Klikatun kirjaimen merkki.</param>
    /// <param name="arvaus">Taulukko, jossa on arvattavan sanan laatikot</param>
    /// <param name="sillanosa">Hävitessä tuhottava sillanosa.</param>
    public void Vertaa(char merkki, PhysicsObject[] arvaus, PhysicsObject sillanosa)
    {
        char klikattu = merkki;
        if (klikattu.Equals(sana[n]))
        {
            Pudotus(sillanosa);
            MerkinLisays(sana[n], arvaus[n]);
            if (n < sana.Length - 1) n++;
            else Lopetus("Voitit!");
        }
        else
        {
            //Pudotus(sillanosa);
        }
    }



    /// <summary>
    /// Vertaa painettua kirjainta ja vuorossa olevaa arvattavan
    /// sanan kirjainta.
    /// </summary>
    /// <param name="merkki">Painetun kirjaimen merkki.</param>
    /// <param name="arvaus">Taulukko, jossa on arvattavan sanan laatikot</param>
    /// <param name="sillanosa">Hävitessä tuhottava sillanosa.</param>
    public void VertaaPainettuKirjain(Key key, PhysicsObject[] arvaus, PhysicsObject sillanosa)
    {
        string kirjain = key.ToString();
        if (key == Key.OemQuotes) kirjain = "Ä";
        if (key == Key.OemTilde) kirjain = "Ö";
        if (key == Key.Aring) kirjain = "Å";
        if (key == Key.F5) return;
        if (key == Key.Escape) return;
        if (key == Key.F1) return;
        char painettu = kirjain[0];
        Vertaa(painettu, arvaus, sillanosa);
    }
}

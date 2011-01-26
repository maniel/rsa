using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSA;
using System.Text.RegularExpressions;

namespace RSA {
    class RSA {
        static int keysize;
        static BigInteger[] klucz = new BigInteger[2];
        static int bctl;
        static void Main(string[] args) {
            Console.WriteLine("chcesz wczytac klucze czy wygenerowac nowe?(w/g)");
            switch (Console.ReadLine()) {
                case "w":
                    readkeys();
                    break;
                case "g":
                    genkeys();
                    break;
            }
            Console.ReadLine();


        }
        static void readkeys() {
            Console.WriteLine("podaj nazwe pliku w ktorym jest klucz");
            String filename = Console.ReadLine();
            FileInfo fi = new FileInfo(filename);
            BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open));
            int rozmiar = Convert.ToInt32(fi.Length / 2);
            klucz[0] = new BigInteger(br.ReadBytes(rozmiar));
            klucz[1] = new BigInteger(br.ReadBytes(rozmiar));
            keysize = rozmiar * 8;
            Console.WriteLine("klucz ma rozmiar {0}b", keysize);
            if (filename.StartsWith("prv")) {
                Console.WriteLine("wykryto klucz prywatny, deszyfrowanie");
                deszyfrowanie();
            } else if (filename.StartsWith("pub")) {
                Console.WriteLine("wykryto klucz publiczny, szyfrowanie");
                szyfrowanie();
            } else {
                Console.WriteLine("podany klucz jest prywatny czy publiczny?(r/u)");
                switch (Console.ReadLine()) {
                    case "r":
                        deszyfrowanie();
                        break;
                    case "u":
                        szyfrowanie();
                        break;
                }
            }
        }

        static void deszyfrowanie() {
            Console.WriteLine("podaj nazwe pliku do odszyfrowania");
            string we = Console.ReadLine();
            string wy = we.Replace(".rsa", "");
            BinaryReader br = new BinaryReader(File.Open(we, FileMode.Open));
            BinaryWriter bw = new BinaryWriter(File.Open(wy, FileMode.Create));
            int blocksize = keysize / 8;
            long blockcount = (br.BaseStream.Length / blocksize);
            bctl = blockcount.ToString().Length;
            for (long i = 0; i < blockcount; i++) {
                byte[] buf = br.ReadBytes(blocksize);
                BigInteger bi = new BigInteger(buf);
                BigInteger bo = decode(bi);
                buf = bo.getBytes();
                if (i != (blockcount - 1))
                    buf = buf.pad(blocksize - 1);
                bw.Write(buf);
                //drawTextProgressBar(i, blockcount);
            }
            bw.Close();
            Console.WriteLine("zapisano pod nazwa {0}", wy);
        }


        static void szyfrowanie() {
            Console.WriteLine("podaj nazwe pliku do zaszyfrowania");
            string we = Console.ReadLine();
            string wy = we + ".rsa";
            BinaryReader br = new BinaryReader(File.Open(we, FileMode.Open));
            BinaryWriter bw = new BinaryWriter(File.Open(wy, FileMode.Create));
            int blocksize = (keysize / 8) - 1;
            long blockcount = (br.BaseStream.Length / blocksize) + 1;
            bctl = blockcount.ToString().Length;
            for (long i = 0; i < blockcount; i++) {
                byte[] buf = br.ReadBytes(blocksize);
                BigInteger bi = new BigInteger(buf);
                BigInteger bo = encode(bi);
                buf = bo.getBytes().pad(blocksize + 1);
                bw.Write(buf);
                //drawTextProgressBar(i, blockcount);
            }
            bw.Close();
            Console.WriteLine("zapisano pod nazwa {0}", wy);
        }


        static void genkeys() {
            int rozmiar = 0;
            do {
                Console.WriteLine("podaj dlugosc klucza");
                String ro = Console.ReadLine();
                rozmiar = Convert.ToInt32(ro);
                if (rozmiar > 2048 || rozmiar < 16)
                    Console.WriteLine("program obsluguje klucze o rozmiarze 16-2048b");
            } while (rozmiar > 2048 || rozmiar < 16);

            Random r = new Random();
            BigInteger p = BigInteger.genPseudoPrime(rozmiar / 2, 1, r);
            BigInteger q = BigInteger.genPseudoPrime(rozmiar / 2, 1, r);
            BigInteger n = p * q;
            BigInteger fi = (p - 1) * (q - 1);
            BigInteger e = fi.genCoPrime(rozmiar, r);
            BigInteger d = e.modInverse(fi);
            String prvfname = "prv." + rozmiar.ToString();
            String pubfname = "pub." + rozmiar.ToString();
            int bytesize = rozmiar / 8;
            BinaryWriter prvbw = new BinaryWriter(File.Open(prvfname, FileMode.Create));
            prvbw.Write(d.getBytes().pad(bytesize));
            prvbw.Write(n.getBytes().pad(bytesize));
            prvbw.Close();
            BinaryWriter pubbw = new BinaryWriter(File.Open(pubfname, FileMode.Create));
            pubbw.Write(e.getBytes().pad(bytesize));
            pubbw.Write(n.getBytes().pad(bytesize));
            pubbw.Close();
            Console.WriteLine("klucz prywatny zostal zapisany pod nazwa {0} a klucz publiczny pod nazwa {1}", prvfname, pubfname);
        }

        static BigInteger encode(BigInteger x) {
            BigInteger e = klucz[0];
            BigInteger n = klucz[1];
            return x.modPow(e, n);
        }

        static BigInteger decode(BigInteger c) {
            BigInteger d = klucz[0];
            BigInteger n = klucz[1];
            return c.modPow(d, n);
        }

        //private static void drawTextProgressBar(long progress, long total) {
        //    //draw empty progress bar
        //    Console.CursorLeft = 0;
        //    Console.Write("["); //start
        //    Console.CursorLeft = 32;
        //    Console.Write("]"); //end
        //    Console.CursorLeft = 1;
        //    float onechunk = 30.0f / total;

        //    //draw filled part
        //    int position = 1;
        //    for (long i = 0; i < onechunk * progress; i++) {
        //        Console.BackgroundColor = ConsoleColor.Gray;
        //        Console.CursorLeft = position++;
        //        Console.Write(" ");
        //    }

        //    //draw unfilled part
        //    for (long i = position; i <= 31; i++) {
        //        Console.BackgroundColor = ConsoleColor.Black;
        //        Console.CursorLeft = position++;
        //        Console.Write(" ");
        //    }

        //    //draw totals
        //    Console.CursorLeft = 35;
        //    Console.BackgroundColor = ConsoleColor.Black;
        //    //Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        //    Console.Write("{0:D" + bctl + "} z {1} {2}bitowych blokow", progress, total, keysize);
        //}
    }
}


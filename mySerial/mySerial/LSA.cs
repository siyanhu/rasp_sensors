using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mySerial
{
    class LSA
    {
        public LSA()
        {
            EX = 0;
            EY = 0;
            EXY = 0;
            EX2 = 0;
            EY2 = 0;
            EX2Y = 0;
            EXY2 = 0;
            EX3 = 0;
            EY3 = 0;
            N = 0;
        }
        private float EX;
        private float EY;
        private float EXY;
        private float EX2;
        private float EY2;
        private float EX2Y;
        private float EXY2;
        private float EX3;
        private float EY3;
        private float N;
        private float C, D, E, G, H;
        public float A, B;

        public void DataUpdate(float x, float y)
        {
            N++;
            EX += x;
            EY += y;
            EXY += x * y;
            EX2 += x * x;
            EY2 += y * y;
            EX2Y += x * x * y;
            EXY2 += x * y * y;
            EX3 += x * x * x;
            EY3 += y * y * y;
            C = N * EX2 - EX * EX;
            D = N * EXY - EX * EY;
            E = N * EX3 + N * EXY2 - (EX2 + EY2) * EX;
            G = N * EY2 - EY * EY;
            H = N * EX2Y + N * EY3 - (EX2 + EY2) * EY;
            A = -0.5f * (H * D - E * G) / (C * G - D * D);
            B = -0.5f * (H * C - E * D) / ( D * D - C * G);
        }
    } 

}

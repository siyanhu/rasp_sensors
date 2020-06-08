#include "LeastSqureCalibration.h"
LeastSqureCalibration::LeastSqureCalibration()
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
void LeastSqureCalibration::DataUpdate(float x, float y)
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

#ifndef LEST_CALI_H
#define LEST_CALI_H
class LeastSqureCalibration
{
  public:
    void DataUpdate(float x, float y);
    LeastSqureCalibration();
    float A, B;
  private:
    float EX;
    float EY;
    float EXY;
    float EX2;
    float EY2;
    float EX2Y;
    float EXY2;
    float EX3;
    float EY3;
    float N;
    float C, D, E, G, H;
} ;
#endif

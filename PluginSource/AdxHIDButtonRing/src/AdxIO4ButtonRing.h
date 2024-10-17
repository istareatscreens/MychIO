#ifndef ADXIO4BUTTONRING_H
#define ADXIO4BUTTONRING_H

#ifdef _WIN32
#define DLL_EXPORT __declspec(dllexport)
#else
#define DLL_EXPORT
#endif

class DLL_EXPORT AdxIo4ButtonRing
{
public:
    void exampleFunction();
};

#endif // ADXIO4BUTTONRING_H
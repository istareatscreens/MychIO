#ifndef ADXHIDBUTTONRING_H
#define ADXHIDBUTTONRING_H

#ifdef _WIN32
#define DLL_EXPORT __declspec(dllexport)
#else
#define DLL_EXPORT
#endif

class DLL_EXPORT AdxHIDButtonRing
{
public:
    void exampleFunction();
};

#endif // ADXHIDBUTTONRING_H
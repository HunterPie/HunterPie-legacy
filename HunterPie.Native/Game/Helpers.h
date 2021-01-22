#pragma once
#include <vector>

template <class T>
T* resolvePtrs(long long* base, std::vector<int> offsets)
{
    for (int offset : offsets)
        base = ((long long*)(*base + offset));

    return reinterpret_cast<T*>(base);
}

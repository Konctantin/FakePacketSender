#include <metahost.h>
#include <Windows.h>
#include <MSCorEE.h>
#include <string>
#pragma comment(lib, "mscoree.lib")
#import "mscorlib.tlb" raw_interfaces_only				\
    high_property_prefixes("_get","_put","_putref")		\
    rename("ReportEvent", "InteropServices_ReportEvent")

BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwAction, LPVOID lpReserved)
{
    return TRUE;
}

extern "C" __declspec(dllexport) HRESULT Inject(_In_ LPCSTR param)
{
    DWORD pReturnValue;
    ICLRMetaHost*    pMetaHost       = NULL;
    ICLRRuntimeInfo* pRuntimeInfo    = NULL;
    ICLRRuntimeHost* pClrRuntimeHost = NULL;

    if (CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&pMetaHost)) != S_OK)
    {
        return -1;
    }
    if (pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&pRuntimeInfo)) != S_OK)
    {
        return -1;
    }
    if (pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, IID_PPV_ARGS(&pClrRuntimeHost)) != S_OK)
    {
        return -1;
    }

    if (pClrRuntimeHost->Start() != S_OK)
    {
        return -1;
    }

    WCHAR wsz[MAX_PATH];
    MultiByteToWideChar(CP_ACP, 0, param, -1, wsz, MAX_PATH);

    HRESULT hr = pClrRuntimeHost->ExecuteInDefaultAppDomain(
        wsz,
        L"FakePacketSender.InjectedEntry",
        L"Run",
        wsz,
        &pReturnValue);

    if (hr = pClrRuntimeHost->Stop() != S_OK)
    {
        _com_error err(hr);
        MessageBox(0, err.ErrorMessage(), L"Error", 0);
        return -1;
    }

    if (hr != S_OK)
    {
        _com_error err(hr);
        MessageBox(0, err.ErrorMessage(), L"Error", 0);
        return -1;
    }

    pMetaHost->Release();
    pRuntimeInfo->Release();
    pClrRuntimeHost->Release();

    return hr;
}
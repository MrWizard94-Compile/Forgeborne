// Original Windows platform bridge. Game rules, JSON and file writes remain in C#.
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shobjidl.h>
#include <wrl/client.h>
#include <cwchar>

using Microsoft::WRL::ComPtr;

extern "C" __declspec(dllexport) HRESULT __cdecl ForgeborneChooseSave(
    int saving, HWND owner, wchar_t* result, int capacity) noexcept
{
    if (!result || capacity < 2) return E_INVALIDARG;
    result[0] = L'\0';
    const HRESULT initialized = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
    if (FAILED(initialized)) return initialized;
    struct Apartment { ~Apartment() { CoUninitialize(); } } apartment;
    ComPtr<IFileDialog> dialog;
    HRESULT status = CoCreateInstance(saving ? __uuidof(FileSaveDialog) : __uuidof(FileOpenDialog),
        nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(dialog.GetAddressOf()));
    if (FAILED(status)) return status;
    FILEOPENDIALOGOPTIONS options{};
    if (FAILED(status = dialog->GetOptions(&options))) return status;
    options |= FOS_FORCEFILESYSTEM | FOS_NOCHANGEDIR | FOS_PATHMUSTEXIST;
    options |= saving ? FOS_OVERWRITEPROMPT | FOS_STRICTFILETYPES : FOS_FILEMUSTEXIST;
    if (FAILED(status = dialog->SetOptions(options))) return status;
    const COMDLG_FILTERSPEC filter[] = { {L"Forgeborne save (*.json)", L"*.json"} };
    if (FAILED(status = dialog->SetFileTypes(1, filter))) return status;
    if (FAILED(status = dialog->SetDefaultExtension(L"json"))) return status;
    if (FAILED(status = dialog->SetTitle(saving ? L"Export Forgeborne save" : L"Import Forgeborne save"))) return status;
    if (FAILED(status = dialog->SetOkButtonLabel(saving ? L"Export save" : L"Import save"))) return status;
    if (saving && FAILED(status = dialog->SetFileName(L"Forgeborne-save.json"))) return status;
    status = dialog->Show(owner);
    if (status == HRESULT_FROM_WIN32(ERROR_CANCELLED)) return S_FALSE;
    if (FAILED(status)) return status;
    ComPtr<IShellItem> item;
    if (FAILED(status = dialog->GetResult(item.GetAddressOf()))) return status;
    struct ShellPath { PWSTR value{}; ~ShellPath() { CoTaskMemFree(value); } } path;
    if (FAILED(status = item->GetDisplayName(SIGDN_FILESYSPATH, &path.value))) return status;
    if (std::wcslen(path.value) >= static_cast<size_t>(capacity)) return HRESULT_FROM_WIN32(ERROR_INSUFFICIENT_BUFFER);
    if (wcscpy_s(result, static_cast<size_t>(capacity), path.value) != 0) return E_INVALIDARG;
    return S_OK;
}

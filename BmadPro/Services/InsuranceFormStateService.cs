using BmadPro.Models;

namespace BmadPro.Services;

public class InsuranceFormStateService
{
    public InsuranceFormModel? FormData { get; private set; }

    public void SetFormData(InsuranceFormModel data)
    {
        FormData = data;
    }
}

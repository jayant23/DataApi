using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess
{
    public enum ExceptionCode
    {
        NoTestedEye,
        PatientDoesNotExist,
        PatientAlresdyExists,
        MoreThanOnePatient,
        MoreThanOneDevice,
        PatientIsAlreadyActive,
        PatientIsAlreadyInactive,
        MissingRequiredField,
        CreateUserFailure,
        PatientDoesNotHaveDevice,
        PatientAlreadyHaveDevice,
        DeviceIsAreadyAssigned,
        AlertAlreadyExists,
        AlertDoesNotExists,
        ClinicAlresdyExists,
        DMCAlresdyExists,
        ECPAlreadyExists,
        CallerAlreadyExists,
        OfficeAlreadyExists
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.DataAccess
{
    public class Enums
    {

    }

    // file type enums
    public enum FileType
    {
        AVI = 1,
        PDF = 2
    }

    // file status 
    public enum FileStatus
    {
        Uploaded = 1,
        InProcess = 2,
        Completed = 3,
        Error = 4
    }

    // Excpetion codes for NOA Analyzer
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

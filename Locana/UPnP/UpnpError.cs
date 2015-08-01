
namespace Kazyx.Uwpmm.UPnP
{
    public enum UpnpError
    {
        InvalidAction = 401,
        InvalidArgs = 402,
        InvalidVar = 404,
        ActionFailed = 501,
        NoSuchObject = 701,
        InvalidCurrentTagValue = 702,
        InvalidNewtagValue = 703,
        Requiredtag = 704,
        ReadOnlyTag = 705,
        ParameterMismatch = 706,
        UnsupportedOrInvalidSearchCriteria = 708,
        UnsupportedOrInvalidSortCriteria = 709,
        NoSuchContainer = 710,
        RestrictedObject = 711,
        BadMetaData = 712,
        RestrictedParentObject = 713,
        NoSuchSourceResource = 714,
        ResourceAccessDenied = 715,
        TransferBusy = 716,
        NoSuchFileTransfer = 717,
        NoSuchDestinationResource = 718,
        DestinationResourceAccessDenied = 719,
        CannotProcessTheRequest = 720,
    }
}

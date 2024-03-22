
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

/// <summary>
/// </summary>
internal partial class CloudbedsDailyOperationsReportManager
{

    struct CheckInsInfo
    {
        public readonly int CheckInsRoomCount = 0;
        public readonly int CheckInsRoomUnassignedCount = 0;
        public readonly IReadOnlyCollection<string>? KnownRoomIds = null;

        public CheckInsInfo(int roomCount, int roomsUnassignedCount, ICollection<string> roomIds)
        {
            this.CheckInsRoomCount = roomCount;
            this.CheckInsRoomUnassignedCount = roomsUnassignedCount;
            this.KnownRoomIds = new List<string>(roomIds).AsReadOnly();
        }

        public CheckInsInfo()
        {

        }
    }


    struct CheckOutsInfo
    {
        public readonly int CheckOutsRoomCount = 0;
        public readonly IReadOnlyCollection<string>? KnownRoomIds = null;

        public CheckOutsInfo(int roomCount, ICollection<string> roomIds)
        {
            this.CheckOutsRoomCount = roomCount;
            this.KnownRoomIds = new List<string>(roomIds).AsReadOnly();
        }

        public CheckOutsInfo()
        {

        }
    }

}


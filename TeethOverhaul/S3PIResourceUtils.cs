namespace TeethOverhaul
{
    public static class S3PIResourceUtils
    {
        public static Sims3.SimIFace.ResourceKey FromS3PIFormatKeyString(string key)
        {
            string[] tgi = key.Replace("0x", "").Split('-');
            return new Sims3.SimIFace.ResourceKey(System.Convert.ToUInt64(tgi[2], 16), System.Convert.ToUInt32(tgi[0], 16), System.Convert.ToUInt32(tgi[1], 16));
        }

        public static string ToS3PIFormatKeyString(this Sims3.SimIFace.ResourceKey key)
        {
            return string.Format("0x{0:X8}-0x{1:X8}-0x{2:X16}", key.TypeId, key.GroupId, key.InstanceId);
        }
    }
}

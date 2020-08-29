namespace TSAsyncTools
{
    /// <summary>
    /// 稼働状態の指定向けinterface
    /// </summary>
    public interface IActivatable
    {
        /// <summary>
        /// 稼働状態
        /// </summary>
        /// <value>false: 非稼働, true: 稼働</value>
        bool Enabled { get; set; }
    }
}
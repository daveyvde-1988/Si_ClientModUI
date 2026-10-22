namespace SiSquadHud;
internal sealed class TrafficMeter
{
    internal long TxPackets,RxPackets,TxBytes,RxBytes,Failures;
    internal long CompanionTxPackets,CompanionRxPackets,CompanionTxBytes,CompanionRxBytes,Rejected,Timeouts,Frames,Replies,Events;
    internal double TxBytesPerSecond,RxBytesPerSecond,LastReplyMs;
    private double at;
    private long previousTx,previousRx;
    internal void Sent(long bytes,bool success){if(!success){Failures++;return;}TxPackets++;TxBytes+=Math.Max(0,bytes);}
    internal void Received(long bytes){RxPackets++;RxBytes+=Math.Max(0,bytes);}
    internal void CompanionSent(int bytes,bool success){if(!success)return;CompanionTxPackets++;CompanionTxBytes+=Math.Max(0,bytes);}
    internal void CompanionReceived(int bytes){CompanionRxPackets++;CompanionRxBytes+=Math.Max(0,bytes);}
    internal void Tick(double now)
    {
        if(now-at<1)return;double elapsed=Math.Max(.001,now-at);
        TxBytesPerSecond=(TxBytes-previousTx)/elapsed;RxBytesPerSecond=(RxBytes-previousRx)/elapsed;
        previousTx=TxBytes;previousRx=RxBytes;at=now;
    }
}

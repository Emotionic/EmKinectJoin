using System.Collections.Generic;

public class CustomData
{

    public bool DoShare; // 共有するかどうか
    public int JoinType; // 観客参加機能のON/OFF, ビットで表現(0ビット目 : いいね, 1 : 拍手, 2 : Kinect)
    public List<string> EnabledLikes; // 使用できるいいね
    public bool IsZNZOVisibled; // 残像を表示するかどうか
    public int TimeLimit; // 時間制限
    public Dictionary<Emotionic.Gesture, Dictionary<Emotionic.Effect, EffectOption>> EffectsCustomize; // エフェクトのカスタマイズデータ(ジェスチャー名, エフェクト名・大きさ・色のデータ)

}

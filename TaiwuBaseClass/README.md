# TaiwuBaseClass
# 基础类

将角色等对象化方便其他mod调用

参考如下代码实现：
```cs
public class Actor
{
    // 角色的id
    public int Id { get; private set; }
    // 第一个为姓，第二个为名
    public Tuple<string, string> Name { get; private set; }

    protected Actor()
    {
    }

    public static Actor fromId(int id)
    {
        var actor = new Actor();
        actor.Id = id;
        // actor.Name = ...
        return actor;
    }
}
```

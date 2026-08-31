using System.Reflection;
using System.Reflection.Emit;

var gameDir = @"D:\Program Files (x86)\steam\steamapps\common\Slay the Spire 2\data_sts2_windows_x86_64";
var ritsuDir = @"C:\Users\13586\.nuget\packages\sts2.ritsulib.compat.0.107.1\0.5.14\lib\net9.0";
var ritsuRuntimeDll = @"D:\Program Files (x86)\steam\steamapps\common\Slay the Spire 2\mods\STS2-RitsuLib\STS2-RitsuLib.dll";
AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
{
    var name = new AssemblyName(args.Name).Name + ".dll";
    foreach (var dir in new[] { gameDir, ritsuDir })
    {
        var path = Path.Combine(dir, name);
        if (File.Exists(path))
        {
            return Assembly.LoadFrom(path);
        }
    }
    return null;
};

var sts2 = Assembly.LoadFrom(Path.Combine(gameDir, "sts2.dll"));
var ritsu = Assembly.LoadFrom(Path.Combine(ritsuDir, "STS2-RitsuLib.dll"));
var ritsuRuntime = Assembly.LoadFrom(ritsuRuntimeDll);

// 运行时 RitsuLib：本地化注册相关方法
foreach (var type in ritsuRuntime.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (method.GetMethodBody() is not { } body)
        {
            continue;
        }

        var bytes = body.GetILAsByteArray();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        if (method.Name.Contains("Loc", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Localiz", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("RegisterCard", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("ContentPack", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"\n===== RitsuLib运行时 本地化相关: {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

DumpType(sts2, "MegaCrit.Sts2.Core.Commands.PowerCmd", includeMethods: true, includeIl: true);
DumpType(sts2, "MegaCrit.Sts2.Core.Factories.CardFactory", includeMethods: true, includeIl: true);
DumpType(sts2, "MegaCrit.Sts2.Core.Commands.CardPileCmd", includeMethods: true, includeIl: true);
DumpType(sts2, "MegaCrit.Sts2.Core.Localization.DynamicVars.StringVar", includeMethods: false, includeIl: false);
DumpType(sts2, "MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet", includeMethods: false, includeIl: false);
DumpType(sts2, "MegaCrit.Sts2.Core.Models.CardModel", includeMethods: false, includeIl: true);
DumpType(sts2, "MegaCrit.Sts2.Core.Localization.LocString", includeMethods: false, includeIl: true);
DumpType(ritsu, "STS2RitsuLib.Scaffolding.Content.ModPowerTemplate", includeMethods: true, includeIl: false);
DumpType(ritsu, "STS2RitsuLib.Scaffolding.Content.ModCardTemplate", includeMethods: true, includeIl: false);

// 嵌套异步状态机的 MoveNext
foreach (var typeName in new[]
{
    "MegaCrit.Sts2.Core.Commands.PowerCmd",
    "MegaCrit.Sts2.Core.Factories.CardFactory",
    "MegaCrit.Sts2.Core.Commands.CardPileCmd",
    "MegaCrit.Sts2.Core.Combat.CombatManager",
    "MegaCrit.Sts2.Core.Hooks.Hook",
})
{
    var outer = sts2.GetType(typeName);
    if (outer is null)
    {
        continue;
    }

    foreach (var nested in outer.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
    {
        var moveNext = nested.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (moveNext is null)
        {
            continue;
        }

        Console.WriteLine($"\n===== MoveNext {nested.FullName} =====");
        DumpIl(moveNext);
    }
}

static void DumpType(Assembly assembly, string typeName, bool includeMethods, bool includeIl)
{
    var type = assembly.GetType(typeName);
    if (type is null)
    {
        Console.WriteLine($"NOT FOUND: {typeName}");
        return;
    }

    Console.WriteLine($"\n=== {typeName} (base: {type.BaseType?.FullName}) ===");
    if (includeMethods)
    {
        foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                     .OrderBy(m => m.Name).ThenBy(m => m.GetParameters().Length))
        {
            Console.WriteLine($"  {(m.IsStatic ? "static " : "")}{m.ReturnType.Name} {m.Name}({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name))})");
        }
    }

    if (includeIl)
    {
        foreach (var methodName in new[] { "get_Title", "get_TitleLocString", "set_StringValue", "get_Item", "Apply", "GetForCombat", "AddGeneratedCardsToCombat", "AddGeneratedCardToCombat", "get_IsUpgraded", "GetFormattedText", "AppendFormatted", "FindExistingInstanceForStacking", "Power" })
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                         .Where(m => m.Name == methodName))
            {
                DumpIl(method);
            }
        }
    }
}

// PowerModel 关键方法
{
    var type = sts2.GetType("MegaCrit.Sts2.Core.Models.PowerModel");
    if (type is not null)
    {
        foreach (var methodName in new[] { "ToMutable", "ApplyInternal", "get_IsVisible", "AssertMutable", "get_IsUpgraded", "SetData", "get_Data", "get_DynamicVars", "InitInternalData", "GetInternalData", "GetData", "MutableClone" })
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                         .Where(m => m.Name == methodName))
            {
                DumpIl(method);
            }
        }
    }
}

// 查找 Power<T>() 工厂方法的定义位置
foreach (var (assembly, label) in new[] { (sts2, "sts2"), (ritsu, "ritsulib") })
{
    foreach (var type in assembly.GetTypes())
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (method.Name == "Power" && method.IsGenericMethodDefinition)
            {
                Console.WriteLine($"\n===== 工厂 Power<T>() 位于 {type.FullName} =====");
                DumpIl(method);
            }
        }
    }
}

// 直接实例化 mod 卡牌，检查 Id/Title 等
{
    var forefingerDll = @"D:\Program Files (x86)\steam\steamapps\common\Slay the Spire 2\mods\Forefinger\Forefinger.dll";
    var forefinger = Assembly.LoadFrom(forefingerDll);
    Console.WriteLine($"\n===== 部署 DLL: {forefingerDll} =====");
    Console.WriteLine("包含的类型:");
    foreach (var t in forefinger.GetTypes().OrderBy(t => t.FullName))
    {
        if (t.FullName?.Contains("Forefinger") == true)
        {
            Console.WriteLine($"  {t.FullName}");
        }
    }
    foreach (var cardTypeName in new[] { "Forefinger.Cards.ForefingerExecuteSkim", "Forefinger.Cards.ForefingerWillOfThePrescript" })
    {
        var type = forefinger.GetType(cardTypeName);
        if (type is null)
        {
            Console.WriteLine($"NOT FOUND: {cardTypeName}");
            continue;
        }

        object? card = null;
        try
        {
            card = Activator.CreateInstance(type);
            Console.WriteLine($"\n=== {cardTypeName} ===");
            var id = type.BaseType?.GetProperty("Id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(card);
            Console.WriteLine($"  Id = {id ?? "<null>"} (type {id?.GetType().FullName ?? "n/a"})");
            if (id is not null)
            {
                foreach (var p in new[] { "Id", "Entry", "Table" })
                {
                    var prop = id.GetType().GetProperty(p, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (prop is not null)
                    {
                        try
                        {
                            Console.WriteLine($"  Id.{p} = {prop.GetValue(id) ?? "<null>"}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Id.{p} threw {ex.GetBaseException().GetType().Name}");
                        }
                    }
                }
            }
            var titleProp = type.BaseType?.GetProperty("Title", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (titleProp is not null)
            {
                try
                {
                    Console.WriteLine($"  Title = {titleProp.GetValue(card)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Title threw {ex.GetBaseException().GetType().Name}: {ex.GetBaseException().Message}");
                }
            }
            var varsProp = type.BaseType?.GetProperty("DynamicVars", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (varsProp is not null)
            {
                try
                {
                    var vars = varsProp.GetValue(card);
                    Console.WriteLine($"  DynamicVars = {vars ?? "<null>"} (type {vars?.GetType().FullName ?? "n/a"})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  DynamicVars threw {ex.GetBaseException().GetType().Name}: {ex.GetBaseException().Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{cardTypeName}: ctor threw {ex.GetBaseException().GetType().Name}: {ex.GetBaseException().Message}");
        }
    }

    Console.WriteLine("\n===== 部署 DLL 中 ForefingerNextExecution 的 IL =====");
    var powerType = forefinger.GetType("Forefinger.Powers.ForefingerNextExecution");
    if (powerType is not null)
    {
        foreach (var method in powerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                     .Where(m => m.Name is "Apply" or "SetSelectedCard" or "BeforeHandDraw" or "get_SelectedCard"))
        {
            DumpIl(method);
        }

        foreach (var nested in powerType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            var moveNext = nested.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (moveNext is not null)
            {
                Console.WriteLine($"\n===== MoveNext {nested.FullName} =====");
                DumpIl(moveNext);
            }
        }
    }
}

// 查找引擎中 BeforeHandDraw 的调用点
foreach (var type in sts2.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (method.Name.Contains("BeforeHandDraw", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("HandDraw", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"\n===== 钩子调用点: {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

// 查找引擎中调用 BeforeHandDraw/AfterCardGeneratedForCombat 钩子的分发方法
foreach (var type in sts2.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (method.GetMethodBody() is not { } body)
        {
            continue;
        }

        var bytes = body.GetILAsByteArray();
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        if (method.Name.Contains("DrawHand", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("DispatchPower", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("Trigger", StringComparison.OrdinalIgnoreCase) && method.Name.Contains("Hand", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"\n===== 分发候选: {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

// CardFactory.FilterForCombat / Rng.NextItem 等
foreach (var (typeName, methodNames) in new (string, string[])[]
{
    ("MegaCrit.Sts2.Core.Factories.CardFactory", new[] { "FilterForCombat", "FilterForPlayerCount", "GetDistinctForCombat" }),
    ("MegaCrit.Sts2.Core.Random.Rng", new[] { "NextItem", "Next", "StableShuffle", "TakeRandom" }),
    ("MegaCrit.Sts2.Core.Localization.LocManager", new[] { "SmartFormat", "GetString", "GetLocalizedString" }),
    ("MegaCrit.Sts2.Core.Localization.LocString", new[] { "GetRawText", "get_LocTable", "get_LocEntryKey", "get_Item" }),
    ("MegaCrit.Sts2.Core.Localization.LocTable", new[] { "IsLocalKey", "GetString", "get_Item" }),
    ("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVarSet", new[] { "get_Item", "Clone", "InitializeWithOwner", "get_Values" }),
    ("MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar", new[] { "Clone", "get_Name" }),
    ("MegaCrit.Sts2.Core.Localization.DynamicVars.StringVar", new[] { "Clone", "get_Name", "set_StringValue" }),
    ("MegaCrit.Sts2.Core.Localization.LocTable", new[] { "GetRawText", "GetString", "get_Item", "TryGetValue" }),
})
{
    var type = sts2.GetType(typeName);
    if (type is null)
    {
        Console.WriteLine($"NOT FOUND: {typeName}");
        continue;
    }

    foreach (var methodName in methodNames)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                     .Where(m => m.Name == methodName))
        {
            Console.WriteLine($"\n--- IL {typeName}.{methodName}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) ---");
            DumpIl(method);
        }
    }
}

// FilterForCombat 的 lambda 谓词
{
    var type = sts2.GetType("MegaCrit.Sts2.Core.Factories.CardFactory");
    foreach (var nested in type?.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic) ?? [])
    {
        foreach (var method in nested.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (method.Name.StartsWith("<FilterForCombat>", StringComparison.Ordinal) || method.Name.StartsWith("<FilterForPlayerCount>", StringComparison.Ordinal))
            {
                Console.WriteLine($"\n--- IL {nested.FullName}.{method.Name} ---");
                DumpIl(method);
            }
        }
    }
}

// 直接按名字找回合开始 / 抽牌入口
foreach (var type in sts2.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (method.Name.Contains("TurnStart", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("StartTurn", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("DrawHand", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("HandleTurn", StringComparison.OrdinalIgnoreCase)
            || method.Name.Contains("BeginTurn", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"\n===== 回合开始候选: {type.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) =====");
            DumpIl(method);
        }
    }
}

// ICombatState.CreateCard 实现
foreach (var type in sts2.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        var ps = method.GetParameters();
        if (method.Name == "CreateCard" && ps.Length == 2 && ps[0].ParameterType.Name == "CardModel" && ps[1].ParameterType.Name == "Player")
        {
            Console.WriteLine($"\n===== CreateCard: {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

// CardRarity / CardType 枚举值
foreach (var typeName in new[] { "MegaCrit.Sts2.Core.Entities.Cards.CardRarity", "MegaCrit.Sts2.Core.Entities.Cards.CardType", "MegaCrit.Sts2.Core.Entities.Powers.PowerInstanceType" })
{
    var type = sts2.GetType(typeName);
    if (type is not null && type.IsEnum)
    {
        Console.WriteLine($"\n{typeName}:");
        foreach (var name in Enum.GetNames(type))
        {
            Console.WriteLine($"  {name} = {Convert.ToInt64(Enum.Parse(type, name))}");
        }
    }
}

// RitsuLib 本地化注册：找处理 "CARD_" / ".title" 的逻辑
foreach (var type in ritsu.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
    {
        if (method.GetMethodBody() is not { } body)
        {
            continue;
        }

        var bytes = body.GetILAsByteArray();
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        if (text.Contains("CARD_") || text.Contains(".title") || text.Contains("cards"))
        {
            Console.WriteLine($"\n===== RitsuLib 本地化相关: {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

// CanBeGeneratedInCombat 默认实现
{
    var type = sts2.GetType("MegaCrit.Sts2.Core.Models.CardModel");
    if (type is not null)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                     .Where(m => m.Name.Contains("CanBeGenerated")))
        {
            Console.WriteLine($"\n===== {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

// 查找 BeforeSideTurnStart(ICombatState, CombatSide, IReadOnlyList<Creature>) 静态分发器
foreach (var type in sts2.GetTypes())
{
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
    {
        var ps = method.GetParameters();
        if ((method.Name == "BeforeSideTurnStart" || method.Name == "BeforeHandDraw" || method.Name == "BeforeHandDrawLate")
            && ps.Length is 3 or 4)
        {
            Console.WriteLine($"\n===== 分发器: {type.FullName}.{method.Name} =====");
            DumpIl(method);
        }
    }
}

// AbstractModel / PowerModel / ModPowerTemplate 构造函数与 InitInternalData 调用
foreach (var typeName in new[]
{
    "MegaCrit.Sts2.Core.Models.AbstractModel",
    "MegaCrit.Sts2.Core.Models.PowerModel",
})
{
    var type = sts2.GetType(typeName);
    if (type is null)
    {
        continue;
    }

    foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
    {
        Console.WriteLine($"\n--- ctor {typeName}({string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name))}) ---");
        DumpIl(ctor);
    }
}

{
    var type = ritsu.GetType("STS2RitsuLib.Scaffolding.Content.ModPowerTemplate");
    if (type is not null)
    {
        foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            Console.WriteLine($"\n--- ctor ModPowerTemplate({string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name))}) ---");
            DumpIl(ctor);
        }
        foreach (var methodName in new[] { "InitInternalData", "GetInternalData", "GetData", "ToMutable", "DeepCloneFields", "AfterCloned", "set_DynamicVars", "get_DynamicVars" })
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                         .Where(m => m.Name == methodName))
            {
                DumpIl(method);
            }
        }
    }
}

static void DumpIl(MethodBase method)
{
    Console.WriteLine($"\n--- IL {method.DeclaringType?.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}) ---");
    var body = method.GetMethodBody();
    if (body is null)
    {
        Console.WriteLine("  <no body>");
        return;
    }

    var bytes = body.GetILAsByteArray();
    var opcodes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(OpCode))
        .Select(f => (OpCode)f.GetValue(null)!)
        .ToDictionary(o => unchecked((ushort)o.Value));

    var module = method.Module;
    var position = 0;
    while (position < bytes.Length)
    {
        var offset = position;
        ushort value = bytes[position++];
        if (value == 0xfe)
        {
            value = (ushort)(0xfe00 | bytes[position++]);
        }

        if (!opcodes.TryGetValue(value, out var opcode))
        {
            Console.WriteLine($"  IL_{offset:x4}: <unknown 0x{value:x4}>");
            break;
        }

        var operand = ReadOperand(bytes, ref position, opcode.OperandType, module, method);
        Console.WriteLine($"  IL_{offset:x4}: {opcode.Name,-12} {operand}");
    }
}

static string ReadOperand(byte[] bytes, ref int position, OperandType operandType, Module module, MethodBase method)
{
    switch (operandType)
    {
        case OperandType.InlineNone:
            return "";
        case OperandType.ShortInlineI:
            return ((sbyte)bytes[position++]).ToString();
        case OperandType.InlineI:
        {
            var v = BitConverter.ToInt32(bytes, position);
            position += 4;
            return v.ToString();
        }
        case OperandType.InlineI8:
        {
            var v = BitConverter.ToInt64(bytes, position);
            position += 8;
            return v.ToString();
        }
        case OperandType.ShortInlineR:
        {
            var v = BitConverter.ToSingle(bytes, position);
            position += 4;
            return v.ToString();
        }
        case OperandType.InlineR:
        {
            var v = BitConverter.ToDouble(bytes, position);
            position += 8;
            return v.ToString();
        }
        case OperandType.ShortInlineVar:
            return "V_" + bytes[position++];
        case OperandType.InlineVar:
        {
            var v = BitConverter.ToUInt16(bytes, position);
            position += 2;
            return "V_" + v;
        }
        case OperandType.ShortInlineBrTarget:
        {
            var delta = (sbyte)bytes[position++];
            return $"IL_{position + delta:x4}";
        }
        case OperandType.InlineBrTarget:
        {
            var delta = BitConverter.ToInt32(bytes, position);
            position += 4;
            return $"IL_{position + delta:x4}";
        }
        case OperandType.InlineSwitch:
        {
            var count = BitConverter.ToInt32(bytes, position);
            position += 4;
            var switchBase = position + count * 4;
            var targets = new string[count];
            for (var i = 0; i < count; i++)
            {
                var delta = BitConverter.ToInt32(bytes, position);
                position += 4;
                targets[i] = $"IL_{switchBase + delta:x4}";
            }
            return "[" + string.Join(", ", targets) + "]";
        }
        case OperandType.InlineString:
        {
            var token = BitConverter.ToInt32(bytes, position);
            position += 4;
            return "\"" + module.ResolveString(token) + "\"";
        }
        case OperandType.InlineField:
        case OperandType.InlineMethod:
        case OperandType.InlineType:
        case OperandType.InlineTok:
        {
            var token = BitConverter.ToInt32(bytes, position);
            position += 4;
            try
            {
                Type[]? typeArgs = method.DeclaringType?.GetGenericArguments();
                Type[]? methodArgs = method.IsGenericMethod ? method.GetGenericArguments() : null;
                return module.ResolveMember(token, typeArgs, methodArgs)?.ToString() ?? ("token " + token);
            }
            catch
            {
                return "token " + token;
            }
        }
        default:
            return $"?{operandType}";
    }
}

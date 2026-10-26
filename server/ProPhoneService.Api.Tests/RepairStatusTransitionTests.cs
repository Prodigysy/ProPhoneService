using ProPhoneService.Domain.Exceptions;
using ProPhoneService.Domain.Model;
using ProPhoneService.Domain.Shared.Enum;

namespace ProPhoneService.Api.Tests;

public class RepairStatusTransitionTests
{
    private static RepairOrder CreateStartedOrder()
    {
        return RepairOrder.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
    }

    public static readonly TheoryData<RepairStatus, RepairStatus> AllowedTransitions = new()
    {
        { RepairStatus.Created, RepairStatus.InDiagnostic },
        { RepairStatus.Created, RepairStatus.Cancelled },
        { RepairStatus.InDiagnostic, RepairStatus.Agreed },
        { RepairStatus.Agreed, RepairStatus.InProgress },
        { RepairStatus.Agreed, RepairStatus.Cancelled },
        { RepairStatus.InProgress, RepairStatus.Ready },
        { RepairStatus.InProgress, RepairStatus.Cancelled },
        { RepairStatus.Ready, RepairStatus.Completed },
    };

    public static readonly TheoryData<RepairStatus, RepairStatus> ForbiddenTransitions = new()
    {
        // Прыжки через этапы основного потока
        { RepairStatus.Created, RepairStatus.Agreed },
        { RepairStatus.Created, RepairStatus.InProgress },
        { RepairStatus.Created, RepairStatus.Ready },
        { RepairStatus.Created, RepairStatus.Completed },
        { RepairStatus.InDiagnostic, RepairStatus.InProgress },
        { RepairStatus.InDiagnostic, RepairStatus.Ready },
        { RepairStatus.InDiagnostic, RepairStatus.Cancelled },
        { RepairStatus.Agreed, RepairStatus.Ready },
        { RepairStatus.Agreed, RepairStatus.Completed },
        { RepairStatus.InProgress, RepairStatus.Completed },
        // Движение назад
        { RepairStatus.InDiagnostic, RepairStatus.Created },
        { RepairStatus.Agreed, RepairStatus.InDiagnostic },
        { RepairStatus.InProgress, RepairStatus.Agreed },
        { RepairStatus.Ready, RepairStatus.InProgress },
        { RepairStatus.Completed, RepairStatus.Ready },
        // Готовый к выдаче нельзя отменить
        { RepairStatus.Ready, RepairStatus.Cancelled },
        // Повторный перевод в тот же статус
        { RepairStatus.Created, RepairStatus.Created },
        { RepairStatus.Ready, RepairStatus.Ready },
        // Терминальные статусы
        { RepairStatus.Completed, RepairStatus.Created },
        { RepairStatus.Completed, RepairStatus.Cancelled },
        { RepairStatus.Cancelled, RepairStatus.Created },
        { RepairStatus.Cancelled, RepairStatus.InDiagnostic },
    };

    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public void ChangeStatus_AllowedTransition_UpdatesStatusAndWritesHistory(RepairStatus from, RepairStatus to)
    {
        var order = CreateOrderInStatus(from);

        order.ChangeStatus(to);

        Assert.Equal(to, order.Status);
        var lastEntry = Assert.Single(order.StatusHistory, x => x.Status == to);
        Assert.Equal(order.Id, lastEntry.RepairOrderId);
    }

    [Theory]
    [MemberData(nameof(ForbiddenTransitions))]
    public void ChangeStatus_ForbiddenTransition_Throws(RepairStatus from, RepairStatus to)
    {
        var order = CreateOrderInStatus(from);
        var historyCountBefore = order.StatusHistory.Count;

        var exception = Assert.Throws<InvalidStatusTransitionException>(() => order.ChangeStatus(to));

        Assert.Contains(from.ToString(), exception.Message);
        Assert.Contains(to.ToString(), exception.Message);
        Assert.Equal(from, order.Status);
        Assert.Equal(historyCountBefore, order.StatusHistory.Count);
    }

    [Fact]
    public void Start_WritesInitialHistoryEntry()
    {
        var order = new RepairOrder { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), DeviceId = Guid.NewGuid(), PreferredVisitAt = DateTime.UtcNow.AddDays(1) };

        order.Start();

        Assert.Equal(RepairStatus.Created, order.Status);
        var entry = Assert.Single(order.StatusHistory);
        Assert.Equal(RepairStatus.Created, entry.Status);
    }

    [Fact]
    public void Start_AlreadyStarted_Throws()
    {
        var order = CreateStartedOrder();

        Assert.Throws<InvalidOperationException>(order.Start);
    }

    [Fact]
    public void ChangeStatus_BeforeStart_Throws()
    {
        var order = new RepairOrder { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), DeviceId = Guid.NewGuid() };

        Assert.Throws<InvalidStatusTransitionException>(() => order.ChangeStatus(RepairStatus.InDiagnostic));

        Assert.Null(order.Status);
        Assert.Empty(order.StatusHistory);
    }

    [Fact]
    public void Start_DoesNotDependOnLoadedHistory_ThrowsOnPersistedOrder()
    {
        // Симуляция загрузки без Include(StatusHistory): история не загружена, но Status уже установлен в БД
        var order = CreateStartedOrder();
        order.StatusHistory.Clear();

        Assert.Throws<InvalidOperationException>(order.Start);
    }

    [Fact]
    public void GetAllowed_ReturnsImmutableWrapper_ExternalMutationCannotChangeRules()
    {
        var allowed = RepairStatusTransitions.GetAllowed(RepairStatus.Created);

        if (allowed is RepairStatus[] mutable)
        {
            mutable[0] = RepairStatus.Completed;
        }

        Assert.Equal(
            new[] { RepairStatus.InDiagnostic, RepairStatus.Cancelled },
            RepairStatusTransitions.GetAllowed(RepairStatus.Created));
    }

    [Fact]
    public void ChangeStatus_Cancelled_KeepsCommentInHistory()
    {
        var order = CreateOrderInStatus(RepairStatus.Agreed);

        order.ChangeStatus(RepairStatus.Cancelled, comment: "Клиент передумал");

        var lastEntry = Assert.Single(order.StatusHistory, x => x.Status == RepairStatus.Cancelled);
        Assert.Equal("Клиент передумал", lastEntry.Comment);
    }

    /// <summary>
    /// Создать заказ и довести его по цепочке до нужного статуса
    /// </summary>
    private static RepairOrder CreateOrderInStatus(RepairStatus target)
    {
        var order = CreateStartedOrder();

        var path = new Dictionary<RepairStatus, RepairStatus[]>
        {
            [RepairStatus.Created] = [],
            [RepairStatus.InDiagnostic] = [RepairStatus.InDiagnostic],
            [RepairStatus.Agreed] = [RepairStatus.InDiagnostic, RepairStatus.Agreed],
            [RepairStatus.InProgress] = [RepairStatus.InDiagnostic, RepairStatus.Agreed, RepairStatus.InProgress],
            [RepairStatus.Ready] = [RepairStatus.InDiagnostic, RepairStatus.Agreed, RepairStatus.InProgress, RepairStatus.Ready],
            [RepairStatus.Completed] = [RepairStatus.InDiagnostic, RepairStatus.Agreed, RepairStatus.InProgress, RepairStatus.Ready, RepairStatus.Completed],
            [RepairStatus.Cancelled] = [RepairStatus.Cancelled],
        };

        foreach (var status in path[target])
        {
            order.ChangeStatus(status);
        }

        return order;
    }
}

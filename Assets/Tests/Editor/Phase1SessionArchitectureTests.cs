#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class Phase1SessionArchitectureTests
{
    private GameObject testObject;
    private AISessionBlueprint blueprint;
    private AISessionDirector director;

    [SetUp]
    public void SetUp()
    {
        blueprint = ScriptableObject.CreateInstance<AISessionBlueprint>();
        blueprint.blueprintId = "test_blueprint";
        blueprint.startChapterId = "start";
        blueprint.chapters = new[]
        {
            new AISessionChapter
            {
                chapterId = "start",
                transitions = new[]
                {
                    new AIChapterTransition { intentTag = "continue", nextChapterId = "next" }
                }
            },
            new AISessionChapter { chapterId = "next" }
        };

        testObject = new GameObject("Phase1SessionArchitectureTests");
        var bridge = testObject.AddComponent<AIBridge>();
        director = testObject.AddComponent<AISessionDirector>();
        SetPrivateField(director, "aiBridge", bridge);

        // The legacy bridge immediately invokes its provider, whose unrelated validation
        // dependencies are intentionally outside Phase 1. Ignore those expected logs here.
        LogAssert.ignoreFailingMessages = true;
        director.StartSession(blueprint);
        LogAssert.ignoreFailingMessages = false;
    }

    [TearDown]
    public void TearDown()
    {
        LogAssert.ignoreFailingMessages = false;
        if (testObject != null)
            UnityEngine.Object.DestroyImmediate(testObject);
        if (blueprint != null)
            UnityEngine.Object.DestroyImmediate(blueprint);
    }

    [Test]
    public void Session_IsImmutableAndStoresOnlyBlueprintId()
    {
        var publicFields = typeof(AIInteractionSession).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var writableProperties = typeof(AIInteractionSession)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.SetMethod != null)
            .ToArray();

        Assert.That(publicFields, Is.Empty);
        Assert.That(writableProperties, Is.Empty);
        Assert.That(typeof(AIInteractionSession).GetProperty("Blueprint"), Is.Null);
        Assert.That(director.CurrentSession.BlueprintId, Is.EqualTo(blueprint.blueprintId));
    }

    [Test]
    public void Session_DefensivelyCopiesHistory()
    {
        var source = new List<AISessionConversationTurn>
        {
            new AISessionConversationTurn("Player", "Hello", "greet")
        };

        var session = new AIInteractionSession(
            "session",
            0,
            "blueprint",
            "chapter",
            AIInteractionState.Running,
            source);

        source.Clear();
        var firstCopy = session.CopyRecentTurns();
        var secondCopy = session.CopyRecentTurns();

        Assert.That(session.RecentTurns.Count, Is.EqualTo(1));
        Assert.That(firstCopy, Is.Not.SameAs(secondCopy));
        Assert.That(firstCopy[0], Is.Not.SameAs(secondCopy[0]));
        Assert.That(session.RecentTurns[0].Text, Is.EqualTo("Hello"));
    }

    [Test]
    public void Snapshot_IsImmutableAndDefensivelyCopiesHistory()
    {
        director.RecordNpcConversationTurn("Hello", "Greeting");
        var snapshot = director.CurrentSnapshot;
        var publicFields = typeof(AISessionSnapshot).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var writableProperties = typeof(AISessionSnapshot)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.SetMethod != null)
            .ToArray();
        var firstCopy = snapshot.CopyRecentTurns();
        var secondCopy = snapshot.CopyRecentTurns();

        Assert.That(publicFields, Is.Empty);
        Assert.That(writableProperties, Is.Empty);
        Assert.That(firstCopy, Is.Not.SameAs(secondCopy));
        Assert.That(firstCopy[0], Is.Not.SameAs(secondCopy[0]));
    }

    [Test]
    public void SnapshotVersion_ChangesAfterEachCommittedMutation()
    {
        var initialVersion = director.CurrentSnapshot.Version;

        Assert.That(director.RecordPlayerConversationTurn(new AIPlayerIntent("continue", "option", "Continue")), Is.True);
        var historyVersion = director.CurrentSnapshot.Version;
        Assert.That(director.AdvanceTurnIndex(), Is.True);
        var turnVersion = director.CurrentSnapshot.Version;

        Assert.That(historyVersion, Is.GreaterThan(initialVersion));
        Assert.That(turnVersion, Is.GreaterThan(historyVersion));
    }

    [Test]
    public void DirectorMutation_ReplacesSessionWithoutChangingPriorInstance()
    {
        var before = director.CurrentSession;

        Assert.That(director.AdvanceTurnIndex(), Is.True);

        Assert.That(director.CurrentSession, Is.Not.SameAs(before));
        Assert.That(before.TurnIndex, Is.EqualTo(0));
        Assert.That(director.CurrentSession.TurnIndex, Is.EqualTo(1));
    }

    [Test]
    public void ChapterResolution_DoesNotMutateSession()
    {
        var session = director.CurrentSession;
        var resolver = new AISessionFlowController();

        var result = resolver.ResolveNextChapter(session, blueprint, "continue");

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.NextChapterId, Is.EqualTo("next"));
        Assert.That(session.CurrentChapterId, Is.EqualTo("start"));
        Assert.That(director.CurrentSession.CurrentChapterId, Is.EqualTo("start"));
    }

    [Test]
    public void LifecycleTransitions_AreOwnedByDirector()
    {
        Assert.That(director.CurrentSession.State, Is.EqualTo(AIInteractionState.Running));
        Assert.That(director.SetSessionWaitingForPlayer(), Is.True);
        Assert.That(director.CurrentSession.State, Is.EqualTo(AIInteractionState.WaitingForPlayer));
        Assert.That(director.SetSessionRunning(), Is.True);
        Assert.That(director.CurrentSession.State, Is.EqualTo(AIInteractionState.Running));

        director.EndSession();

        Assert.That(director.CurrentSession.State, Is.EqualTo(AIInteractionState.Finished));
        Assert.That(director.HasActiveSession, Is.False);
        Assert.That(director.AdvanceTurnIndex(), Is.False);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, $"Expected private field '{fieldName}'.");
        field.SetValue(target, value);
    }
}
#endif

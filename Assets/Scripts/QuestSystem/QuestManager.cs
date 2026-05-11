using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using BNG;

namespace QuestSystem {
    public class QuestManager : MonoBehaviour {
        [Header("Quests")]
        public List<QuestDefinition> quests = new List<QuestDefinition>();
        public bool autoStartFirstQuest = true;
        public int activeQuestIndex = -1;

        [Header("Grabbers")]
        public Grabber leftGrabber;
        public Grabber rightGrabber;

        [Header("Player")]
        public Transform playerTarget;
        public bool autoFindPlayer = true;

        [Header("Events")]
        public UnityEvent onQuestStarted;
        public UnityEvent onQuestProgressChanged;
        public UnityEvent onQuestCompleted;

        private readonly List<ObjectiveProgress> activeObjectives = new List<ObjectiveProgress>();
        private readonly Dictionary<Damageable, UnityAction> enemyDeathHandlers = new Dictionary<Damageable, UnityAction>();

        [Serializable]
        public class QuestDefinition {
            public string questId = "quest";
            public string title = "Quest";
            [TextArea]
            public string description;
            public bool autoStart;
            public List<QuestObjectiveConfig> objectives = new List<QuestObjectiveConfig>();
        }

        public enum QuestObjectiveType {
            CollectItem,
            EquipItem,
            KillEnemies,
            ReachLocation
        }

        [Serializable]
        public class QuestObjectiveConfig {
            public QuestObjectiveType type = QuestObjectiveType.CollectItem;
            public string title = "Objective";
            [TextArea]
            public string description;

            [Header("Item Targets")]
            public Grabbable targetItem;
            public QuestWeaponId targetWeapon;

            [Header("Kill Targets")]
            public List<Damageable> enemies = new List<Damageable>();
            public int requiredKills = 1;
            public bool requireSpecificWeapon = false;

            [Header("Reach Location")]
            public Transform locationTarget;
            public float reachRadius = 1.5f;
            public float requiredStaySeconds = 0f;
        }

        private class ObjectiveProgress {
            public QuestObjectiveConfig config;
            public int currentCount;
            public bool completed;
            public HashSet<Damageable> killedTargets = new HashSet<Damageable>();
            public float timeInRange;
            public float lastDistance = float.PositiveInfinity;
        }

        public struct ObjectiveStatus {
            public QuestObjectiveType type;
            public string title;
            public string description;
            public bool completed;
            public int currentCount;
            public int requiredCount;
            public bool requireSpecificWeapon;
            public string weaponName;
            public string targetName;
            public float distance;
            public float reachRadius;
            public float timeInRange;
            public float requiredStaySeconds;
        }

        private void Start() {
            if (quests.Count == 0) {
                return;
            }

            int indexToStart = -1;
            for (int i = 0; i < quests.Count; i++) {
                if (quests[i].autoStart) {
                    indexToStart = i;
                    break;
                }
            }

            if (indexToStart == -1 && autoStartFirstQuest) {
                indexToStart = 0;
            }

            if (indexToStart >= 0) {
                StartQuest(indexToStart);
            }
        }

        private void Update() {
            if (activeQuestIndex < 0 || activeQuestIndex >= quests.Count) {
                return;
            }

            if (autoFindPlayer) {
                ResolvePlayerTarget();
            }

            bool changed = false;
            foreach (ObjectiveProgress progress in activeObjectives) {
                if (progress.completed || progress.config.type != QuestObjectiveType.ReachLocation) {
                    continue;
                }

                if (progress.config.locationTarget == null || playerTarget == null) {
                    continue;
                }

                float distance = Vector3.Distance(playerTarget.position, progress.config.locationTarget.position);
                progress.lastDistance = distance;

                float radius = Mathf.Max(0.1f, progress.config.reachRadius);
                if (distance <= radius) {
                    if (progress.config.requiredStaySeconds <= 0f) {
                        progress.completed = true;
                        changed = true;
                        continue;
                    }

                    progress.timeInRange += Time.deltaTime;
                    if (progress.timeInRange >= progress.config.requiredStaySeconds) {
                        progress.completed = true;
                        changed = true;
                    }
                }
                else {
                    progress.timeInRange = 0f;
                }
            }

            if (changed) {
                HandleQuestProgressChanged();
            }
        }

        private void OnDisable() {
            UnregisterGrabbers();
            UnregisterEnemies();
        }

        public void StartQuest(int questIndex) {
            if (questIndex < 0 || questIndex >= quests.Count) {
                return;
            }

            UnregisterGrabbers();
            UnregisterEnemies();

            activeQuestIndex = questIndex;
            activeObjectives.Clear();

            QuestDefinition quest = quests[activeQuestIndex];
            foreach (QuestObjectiveConfig config in quest.objectives) {
                ObjectiveProgress progress = new ObjectiveProgress {
                    config = config,
                    currentCount = 0,
                    completed = false,
                    timeInRange = 0f,
                    lastDistance = float.PositiveInfinity
                };
                activeObjectives.Add(progress);
            }

            RegisterGrabbers();
            RegisterEnemies();
            RefreshEquipObjectives();

            onQuestStarted?.Invoke();
            onQuestProgressChanged?.Invoke();
        }

        public QuestDefinition GetActiveQuest() {
            if (activeQuestIndex < 0 || activeQuestIndex >= quests.Count) {
                return null;
            }

            return quests[activeQuestIndex];
        }

        public int GetObjectiveCount() {
            return activeObjectives.Count;
        }

        public ObjectiveStatus GetObjectiveStatus(int index) {
            ObjectiveStatus status = new ObjectiveStatus();
            if (index < 0 || index >= activeObjectives.Count) {
                return status;
            }

            ObjectiveProgress progress = activeObjectives[index];
            QuestObjectiveConfig config = progress.config;
            status.type = config.type;
            status.title = !string.IsNullOrEmpty(config.title) ? config.title : config.type.ToString();
            status.description = config.description;
            status.completed = progress.completed;
            status.currentCount = progress.currentCount;
            status.requiredCount = Mathf.Max(1, config.requiredKills);
            status.requireSpecificWeapon = config.requireSpecificWeapon && config.targetWeapon != null;
            status.weaponName = GetWeaponName(config.targetWeapon);
            status.targetName = GetTargetName(config);
            status.distance = progress.lastDistance;
            status.reachRadius = Mathf.Max(0.1f, config.reachRadius);
            status.timeInRange = progress.timeInRange;
            status.requiredStaySeconds = Mathf.Max(0f, config.requiredStaySeconds);
            return status;
        }

        public string GetQuestSummary() {
            if (activeQuestIndex < 0 || activeQuestIndex >= quests.Count) {
                return "No active quest.";
            }

            QuestDefinition quest = quests[activeQuestIndex];
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(quest.title);

            if (!string.IsNullOrEmpty(quest.description)) {
                builder.AppendLine(quest.description);
            }

            builder.AppendLine("Objectives:");

            foreach (ObjectiveProgress progress in activeObjectives) {
                builder.Append("- ");
                builder.Append(progress.completed ? "[x] " : "[ ] ");
                builder.Append(GetObjectiveText(progress));
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private string GetObjectiveText(ObjectiveProgress progress) {
            QuestObjectiveConfig config = progress.config;
            string title = !string.IsNullOrEmpty(config.title) ? config.title : config.type.ToString();

            if (config.type == QuestObjectiveType.KillEnemies) {
                int required = Mathf.Max(1, config.requiredKills);
                string weaponNote = string.Empty;

                if (config.requireSpecificWeapon && config.targetWeapon != null) {
                    string weaponName = !string.IsNullOrEmpty(config.targetWeapon.displayName)
                        ? config.targetWeapon.displayName
                        : config.targetWeapon.name;
                    weaponNote = " with " + weaponName;
                }

                return title + " (" + progress.currentCount + "/" + required + ")" + weaponNote;
            }

            if (config.type == QuestObjectiveType.ReachLocation) {
                string targetName = GetTargetName(config);
                string distanceText = float.IsPositiveInfinity(progress.lastDistance)
                    ? string.Empty
                    : " (" + progress.lastDistance.ToString("0.0") + "m)";
                return title + " - " + targetName + distanceText;
            }

            return title;
        }

        private string GetTargetName(QuestObjectiveConfig config) {
            if (config.type == QuestObjectiveType.ReachLocation && config.locationTarget != null) {
                QuestLocationTarget locationTarget = config.locationTarget.GetComponent<QuestLocationTarget>();
                if (locationTarget != null && !string.IsNullOrEmpty(locationTarget.displayName)) {
                    return locationTarget.displayName;
                }

                return config.locationTarget.name;
            }

            if (config.targetItem != null) {
                return config.targetItem.name;
            }

            if (config.targetWeapon != null) {
                return GetWeaponName(config.targetWeapon);
            }

            return "Target";
        }

        private string GetWeaponName(QuestWeaponId weaponId) {
            if (weaponId == null) {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(weaponId.displayName)) {
                return weaponId.displayName;
            }

            return weaponId.name;
        }

        private void RegisterGrabbers() {
            RegisterGrabber(leftGrabber);
            RegisterGrabber(rightGrabber);
        }

        private void UnregisterGrabbers() {
            UnregisterGrabber(leftGrabber);
            UnregisterGrabber(rightGrabber);
        }

        private void RegisterGrabber(Grabber grabber) {
            if (grabber == null) {
                return;
            }

            grabber.onAfterGrabEvent.AddListener(HandleGrabbed);
            grabber.onReleaseEvent.AddListener(HandleReleased);
        }

        private void UnregisterGrabber(Grabber grabber) {
            if (grabber == null) {
                return;
            }

            grabber.onAfterGrabEvent.RemoveListener(HandleGrabbed);
            grabber.onReleaseEvent.RemoveListener(HandleReleased);
        }

        private void RegisterEnemies() {
            foreach (ObjectiveProgress progress in activeObjectives) {
                if (progress.config.type != QuestObjectiveType.KillEnemies) {
                    continue;
                }

                foreach (Damageable enemy in progress.config.enemies) {
                    if (enemy == null || enemyDeathHandlers.ContainsKey(enemy)) {
                        continue;
                    }

                    UnityAction handler = () => HandleEnemyDestroyed(enemy);
                    enemy.onDestroyed.AddListener(handler);
                    enemyDeathHandlers.Add(enemy, handler);
                }
            }
        }

        private void UnregisterEnemies() {
            foreach (KeyValuePair<Damageable, UnityAction> entry in enemyDeathHandlers) {
                Damageable enemy = entry.Key;
                if (enemy == null) {
                    continue;
                }

                enemy.onDestroyed.RemoveListener(entry.Value);
            }

            enemyDeathHandlers.Clear();
        }

        private void HandleGrabbed(Grabbable grabbed) {
            if (grabbed == null) {
                return;
            }

            bool changed = false;

            foreach (ObjectiveProgress progress in activeObjectives) {
                if (progress.completed) {
                    continue;
                }

                if (progress.config.type == QuestObjectiveType.CollectItem) {
                    if (MatchesTarget(progress.config, grabbed)) {
                        progress.completed = true;
                        changed = true;
                    }
                }
                else if (progress.config.type == QuestObjectiveType.EquipItem) {
                    if (MatchesTarget(progress.config, grabbed)) {
                        progress.completed = true;
                        changed = true;
                    }
                }
            }

            if (changed) {
                HandleQuestProgressChanged();
            }
        }

        private void HandleReleased(Grabbable released) {
            RefreshEquipObjectives();
        }

        private void RefreshEquipObjectives() {
            bool changed = false;

            foreach (ObjectiveProgress progress in activeObjectives) {
                if (progress.completed || progress.config.type != QuestObjectiveType.EquipItem) {
                    continue;
                }

                if (IsEquipped(progress.config)) {
                    progress.completed = true;
                    changed = true;
                }
            }

            if (changed) {
                HandleQuestProgressChanged();
            }
        }

        private bool IsEquipped(QuestObjectiveConfig config) {
            return IsGrabberHolding(leftGrabber, config) || IsGrabberHolding(rightGrabber, config);
        }

        private bool IsGrabberHolding(Grabber grabber, QuestObjectiveConfig config) {
            if (grabber == null || grabber.HeldGrabbable == null) {
                return false;
            }

            return MatchesTarget(config, grabber.HeldGrabbable);
        }

        private bool MatchesTarget(QuestObjectiveConfig config, Grabbable grabbed) {
            if (config.targetItem != null) {
                return grabbed == config.targetItem;
            }

            if (config.targetWeapon != null) {
                QuestWeaponId weaponId = grabbed.GetComponentInParent<QuestWeaponId>();
                return weaponId == config.targetWeapon;
            }

            return false;
        }

        private void HandleEnemyDestroyed(Damageable enemy) {
            if (enemy == null) {
                return;
            }

            bool changed = false;

            foreach (ObjectiveProgress progress in activeObjectives) {
                if (progress.completed || progress.config.type != QuestObjectiveType.KillEnemies) {
                    continue;
                }

                if (!IsEnemyInObjective(progress, enemy)) {
                    continue;
                }

                if (progress.config.requireSpecificWeapon && progress.config.targetWeapon != null) {
                    QuestWeaponId lastWeapon = QuestCombatTracker.GetLastHitWeapon(enemy);
                    if (lastWeapon != progress.config.targetWeapon) {
                        continue;
                    }
                }

                if (progress.killedTargets.Add(enemy)) {
                    progress.currentCount++;
                    if (progress.currentCount >= Mathf.Max(1, progress.config.requiredKills)) {
                        progress.completed = true;
                    }
                    changed = true;
                }
            }

            QuestCombatTracker.Clear(enemy);

            if (changed) {
                HandleQuestProgressChanged();
            }
        }

        private bool IsEnemyInObjective(ObjectiveProgress progress, Damageable enemy) {
            List<Damageable> enemies = progress.config.enemies;
            if (enemies == null || enemies.Count == 0) {
                return true;
            }

            return enemies.Contains(enemy);
        }

        private void HandleQuestProgressChanged() {
            onQuestProgressChanged?.Invoke();

            if (AreAllObjectivesComplete()) {
                onQuestCompleted?.Invoke();
            }
        }

        private bool AreAllObjectivesComplete() {
            foreach (ObjectiveProgress progress in activeObjectives) {
                if (!progress.completed) {
                    return false;
                }
            }

            return activeObjectives.Count > 0;
        }

        private bool ResolvePlayerTarget() {
            if (playerTarget != null) {
                return true;
            }

            if (Camera.main != null) {
                playerTarget = Camera.main.transform;
                return true;
            }

            GameObject taggedCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (taggedCamera != null) {
                playerTarget = taggedCamera.transform;
                return true;
            }

            return false;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using BNG;

namespace QuestSystem {
    public static class QuestCombatTracker {
        private static readonly Dictionary<Damageable, QuestWeaponId> LastHitByWeapon = new Dictionary<Damageable, QuestWeaponId>();

        public static void RegisterHit(Damageable target, QuestWeaponId weapon) {
            if (target == null || weapon == null) {
                return;
            }

            LastHitByWeapon[target] = weapon;
        }

        public static QuestWeaponId GetLastHitWeapon(Damageable target) {
            if (target == null) {
                return null;
            }

            LastHitByWeapon.TryGetValue(target, out QuestWeaponId weapon);
            return weapon;
        }

        public static void Clear(Damageable target) {
            if (target == null) {
                return;
            }

            LastHitByWeapon.Remove(target);
        }
    }
}

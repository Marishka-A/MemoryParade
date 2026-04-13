using UnityEngine;
using System.Collections.Generic;

namespace Assets.MemoryParade.Scripts.Game.Gameplay.MapGeneration
{
    public class MapInitializer : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject wallPrefab;
        public GameObject horizontalCorridorPrefab;
        public GameObject verticalCorridorPrefab;
        public GameObject floorPrefab;
        public GameObject doorPrefab;
        public GameObject wallAnglePrefab;
        public GameObject emptyWallPrefab;

        [Header("Player")]
        public GameObject player;

        [Header("Enemies")]
        public GameObject SlimePrefab;
        public GameObject FlamePrefab;

        [Header("Enemy counts")]
        public int slimeCount = 10;
        public int flameCount = 5;

        [Header("Mana orbs")]
        public GameObject manaOrbPrefab;
        public int manaOrbCount = 5;

        public static Vector2 CellSize = new Vector2(1, 1);

        private List<Room> generatedRooms;

        void Start()
        {
            ResetPreviousMapState();

            MapRenderer.WallPrefab = wallPrefab;
            MapRenderer.HorizontalCorridorPrefab = horizontalCorridorPrefab;
            MapRenderer.VerticalCorridorPrefab = verticalCorridorPrefab;
            MapRenderer.FloorPrefab = floorPrefab;
            MapRenderer.DoorPrefab = doorPrefab;
            MapRenderer.WallAnglePrefab = wallAnglePrefab;
            MapRenderer.EmptyWallPrefab = emptyWallPrefab;

            generatedRooms = GeneratingMap();

            if (generatedRooms != null && generatedRooms.Count > 0)
            {
                GameObject realPlayer = GameObject.FindWithTag("Player");

                if (realPlayer != null)
                {
                    SpawnPlayerInRoom(realPlayer, generatedRooms[0]);
                }
                else
                {
                    Debug.LogError("Игрок с тегом Player не найден");
                }
            }

            SpawnEnemiesAndMana();
        }

        private void ResetPreviousMapState()
        {
            if (MapRenderer.MapParent != null)
            {
                if (MapRenderer.MapParent.gameObject != null)
                {
                    Destroy(MapRenderer.MapParent.gameObject);
                }

                MapRenderer.MapParent = null;
            }

            GameObject oldRoot = GameObject.Find("MapRoot");
            if (oldRoot != null)
            {
                Destroy(oldRoot);
            }

            generatedRooms = null;
        }

        private void SpawnEnemiesAndMana()
        {
            if (generatedRooms == null || generatedRooms.Count == 0)
            {
                Debug.LogError("Комнаты не сгенерированы, нечего спавнить");
                return;
            }

            SpawnObjectsInRooms(SlimePrefab, slimeCount, "Slime");
            SpawnObjectsInRooms(FlamePrefab, flameCount, "Flame");
            SpawnObjectsInRooms(manaOrbPrefab, manaOrbCount, "Mana Orb");
        }

        private void SpawnObjectsInRooms(GameObject prefab, int count, string debugName)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{debugName} prefab не назначен");
                return;
            }

            if (count <= 0)
            {
                Debug.LogWarning($"{debugName} count <= 0");
                return;
            }

            int spawned = 0;
            int attempts = 0;
            int maxAttempts = count * 20;

            while (spawned < count && attempts < maxAttempts)
            {
                attempts++;

                Room room = generatedRooms[Random.Range(0, generatedRooms.Count)];
                var (centerX, centerY) = room.Center();

                int offsetX = Random.Range(-2, 3);
                int offsetY = Random.Range(-2, 3);

                Vector3 spawnPos = new Vector3(
                    (centerX + offsetX) * CellSize.x,
                    -(centerY + offsetY) * CellSize.y,
                    0f
                );

                Collider2D hit = Physics2D.OverlapCircle(spawnPos, 0.6f);
                if (hit != null)
                    continue;

                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
                Debug.Log($"{debugName} spawned: {obj.name} at {spawnPos}");

                spawned++;
            }

            if (spawned < count)
            {
                Debug.LogWarning($"Не удалось заспавнить все объекты {debugName}. Создано: {spawned} из {count}");
            }
        }

        List<Room> GeneratingMap()
        {
            List<Room> spawnRooms = MapGenerator.GenerateAndRenderMap();

            while (CheckRegeneration())
            {
                DestroyMap();
                spawnRooms = MapGenerator.GenerateAndRenderMap();
            }

            HandleCollisions();
            OffFloorCollider();

            return spawnRooms;
        }

        void SpawnPlayerInRoom(GameObject playerObject, Room spawnRoom)
        {
            if (playerObject == null)
            {
                Debug.LogError("В MapInitializer не назначен player");
                return;
            }

            if (spawnRoom == null)
            {
                Debug.LogError("Комната для спавна игрока не найдена");
                return;
            }

            var (centerX, centerY) = spawnRoom.Center();
            Vector3 spawnPosition = new Vector3(centerX * CellSize.x, -centerY * CellSize.y, 0f);

            playerObject.transform.position = spawnPosition;

            Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            var battleSystem = playerObject.GetComponent<BattleSystem>();
            if (battleSystem != null && battleSystem.battleCanvas == null)
            {
                Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (canvas.name == "BattleCanvas")
                    {
                        battleSystem.battleCanvas = canvas.gameObject;
                        break;
                    }
                }

                if (battleSystem.battleCanvas == null)
                {
                    Debug.LogWarning("BattleCanvas не найден при инициализации игрока");
                }
            }
        }

        void HandleCollisions()
        {
            foreach (var wall in GameObject.FindGameObjectsWithTag("Wall"))
            {
                BoxCollider2D wallCollider = wall.GetComponent<BoxCollider2D>();
                if (wallCollider == null) continue;

                Vector2 wallCenter = wallCollider.bounds.center;
                Vector2 wallSize = wallCollider.bounds.size;

                Collider2D[] overlaps = Physics2D.OverlapBoxAll(wallCenter, wallSize, 0);
                foreach (var overlap in overlaps)
                {
                    if (overlap.gameObject == wall) continue;

                    if (overlap.CompareTag("Corridor") || overlap.CompareTag("Floor"))
                    {
                        BoxCollider2D overlapCollider = overlap.GetComponent<BoxCollider2D>();
                        if (overlapCollider != null)
                        {
                            Vector2 overlapCenter = overlapCollider.bounds.center;
                            Vector2 overlapSize = overlapCollider.bounds.size;

                            float tolerance = 0.01f;
                            bool positionsMatch = Vector2.Distance(overlapCenter, wallCenter) < tolerance;
                            bool sizesMatch =
                                Mathf.Abs(overlapSize.x - wallSize.x) < tolerance &&
                                Mathf.Abs(overlapSize.y - wallSize.y) < tolerance;

                            if (positionsMatch && sizesMatch)
                            {
                                Destroy(wall);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OffFloorCollider()
        {
            foreach (var floor in GameObject.FindGameObjectsWithTag("Floor"))
            {
                var collider = floor.GetComponent<BoxCollider2D>();
                if (collider != null)
                    collider.enabled = false;
            }
        }

        bool CheckRegeneration()
        {
            foreach (var corner in GameObject.FindGameObjectsWithTag("Corner"))
            {
                BoxCollider2D cornerCollider = corner.GetComponent<BoxCollider2D>();
                if (cornerCollider == null) continue;

                Vector2 cornerCenter = cornerCollider.bounds.center;
                Vector2 cornerSize = cornerCollider.bounds.size;

                Collider2D[] overlaps = Physics2D.OverlapBoxAll(cornerCenter, cornerSize, 0);
                foreach (var overlap in overlaps)
                {
                    if (overlap.gameObject == corner) continue;

                    if (overlap.CompareTag("Corridor"))
                    {
                        BoxCollider2D overlapCollider = overlap.GetComponent<BoxCollider2D>();
                        if (overlapCollider != null)
                        {
                            Vector2 overlapCenter = overlapCollider.bounds.center;
                            Vector2 overlapSize = overlapCollider.bounds.size;

                            float tolerance = 0.01f;
                            bool positionsMatch = Vector2.Distance(overlapCenter, cornerCenter) < tolerance;
                            bool sizesMatch =
                                Mathf.Abs(overlapSize.x - cornerSize.x) < tolerance &&
                                Mathf.Abs(overlapSize.y - cornerSize.y) < tolerance;

                            if (positionsMatch && sizesMatch)
                            {
                                Debug.LogWarning("Пересечение коридора с углом комнаты");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        void DestroyMap()
        {
            if (MapRenderer.MapParent != null)
            {
                if (MapRenderer.MapParent.gameObject != null)
                {
                    Destroy(MapRenderer.MapParent.gameObject);
                }

                MapRenderer.MapParent = null;
            }

            GameObject oldRoot = GameObject.Find("MapRoot");
            if (oldRoot != null)
            {
                Destroy(oldRoot);
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

namespace Assets.MemoryParade.Scripts.Game.Gameplay.MapGeneration
{
    /// <summary>
    /// Загружает префабы карты
    /// </summary>
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

        // Игрок на сцене
        public GameObject player;

        // Враги
        public GameObject SlimePrefab;
        public GameObject MummyPrefab;

        public static Vector2 CellSize = new Vector2(1, 1);

        void Start()
        {
            MapRenderer.WallPrefab = wallPrefab;
            MapRenderer.HorizontalCorridorPrefab = horizontalCorridorPrefab;
            MapRenderer.VerticalCorridorPrefab = verticalCorridorPrefab;
            MapRenderer.FloorPrefab = floorPrefab;
            MapRenderer.DoorPrefab = doorPrefab;
            MapRenderer.WallAnglePrefab = wallAnglePrefab;
            MapRenderer.EmptyWallPrefab = emptyWallPrefab;

            List<Room> rooms = GeneratingMap();

            if (rooms != null && rooms.Count > 0)
            {
                SpawnPlayerInRoom(player, rooms[0]);
            }

            // Пока оставляем врагов выключенными, чтобы не возвращать вылет
            // EnemyPositionGenerator.SpawnEnemies(SlimePrefab, 5, rooms, CellSize);
            // EnemyPositionGenerator.SpawnEnemies(MummyPrefab, 5, rooms, CellSize);
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
            playerObject.transform.position = new Vector3(centerX * CellSize.x, -centerY * CellSize.y, 0);

            // Дополнительно пробуем назначить BattleCanvas игроку
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
            foreach (var obj in GameObject.FindGameObjectsWithTag("Wall")) Destroy(obj);
            foreach (var obj in GameObject.FindGameObjectsWithTag("Floor")) Destroy(obj);
            foreach (var obj in GameObject.FindGameObjectsWithTag("Corridor")) Destroy(obj);
            foreach (var obj in GameObject.FindGameObjectsWithTag("Corner")) Destroy(obj);
        }

        void DestroyEnemy()
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(obj);
        }
    }
}
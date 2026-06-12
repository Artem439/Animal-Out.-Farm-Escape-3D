using Game.Scripts.Resources.Environment;
using UnityEngine;

namespace Game.Scripts.Environment.Grid.Configuration
{
    public class FieldLayout : MonoBehaviour
    {
        private const float OffsetY = 0.01f;
        private const float HalfDivider = 2f;

        [SerializeField] private FieldConfiguration _configuration;

        public FieldConfiguration Configuration => _configuration;
        public int Width => _configuration.Width;
        public int Length => _configuration.Length;
        public float CellSize => _configuration.CellSize;
        public int PerimeterBorderOffset => _configuration.PerimeterBorderOffset;

        public void ApplyConfiguration(FieldConfiguration configuration)
        {
            _configuration = configuration;
        }

        public float GetFrontBorderZ()
        {
            float halfLength = (Length + PerimeterBorderOffset - 1) * CellSize / HalfDivider;
            return transform.position.z + halfLength;
        }

        public Vector3 GetCellsStartPosition()
        {
            float widthOffset = (Width - 1) * CellSize / HalfDivider;
            float lengthOffset = (Length - 1) * CellSize / HalfDivider;

            return new Vector3(
                transform.position.x - widthOffset,
                transform.position.y + OffsetY,
                transform.position.z + lengthOffset);
        }

        public Vector3 GetCellPosition(int row, int column)
        {
            Vector3 startPosition = GetCellsStartPosition();

            return new Vector3(
                startPosition.x + column * CellSize,
                startPosition.y,
                startPosition.z - row * CellSize);
        }

        public Vector3 GetPerimeterStartPosition()
        {
            float widthOffset = (Width + PerimeterBorderOffset - 1) * CellSize / HalfDivider;
            float lengthOffset = (Length + PerimeterBorderOffset - 1) * CellSize / HalfDivider;

            return new Vector3(
                transform.position.x - widthOffset,
                transform.position.y + OffsetY,
                transform.position.z + lengthOffset);
        }

        public Vector3 GetPerimeterEntryPoint(Vector3 position, Vector3 direction)
        {
            direction = new Vector3(direction.x, 0f, direction.z).normalized;

            float halfWidth = GetPerimeterHalfWidth();
            float halfLength = GetPerimeterHalfLength();

            float minX = transform.position.x - halfWidth;
            float maxX = transform.position.x + halfWidth;
            float minZ = transform.position.z - halfLength;
            float maxZ = transform.position.z + halfLength;

            float entryX;
            float entryZ;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
            {
                entryX = direction.x > 0 ? maxX : minX;
                float t = (entryX - position.x) / direction.x;
                entryZ = Mathf.Clamp(position.z + t * direction.z, minZ, maxZ);
            }
            else
            {
                entryZ = direction.z > 0 ? maxZ : minZ;
                float t = (entryZ - position.z) / direction.z;
                entryX = Mathf.Clamp(position.x + t * direction.x, minX, maxX);
            }

            return new Vector3(entryX, position.y, entryZ);
        }

        public float GetPerimeterHalfWidth()
        {
            return (Width + PerimeterBorderOffset - 1) * CellSize * 0.5f;
        }

        public float GetPerimeterHalfLength()
        {
            return (Length + PerimeterBorderOffset - 1) * CellSize * 0.5f;
        }
    }
}

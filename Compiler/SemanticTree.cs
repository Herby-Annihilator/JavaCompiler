using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class SemanticTree
    {
        private SemanticTree _parent;
        private SemanticTree _leftChild;
        private SemanticTree _rightChild;  // поле будет использоваться для хранения значения экземпляра пользовательского типа
        
        public SemanticTree Right { get => _rightChild; set => _rightChild = value; }
        public SemanticTree Left { get => _leftChild; set => _leftChild = value; }
        public SemanticTree Parent { get => _parent; set => _parent = value; }
        
        public EntityData Data { get; set; }  // данные о сущности
        /// <summary>
        /// Флаг интерпртетации - по умолчанию = <see langword="true"/>
        /// </summary>
        public static bool IsInterpret { get; set; } = true; // флаг интерпретации

        public SemanticTree CurrentVertex { get; set; }


        #region TreeMethods
        public SemanticTree(SemanticTree parent, SemanticTree leftChild, SemanticTree rightChild, EntityData data)
        {
            _parent = parent;
            _leftChild = leftChild;
            _rightChild = rightChild;
            Data = data;
            CurrentVertex = this;
        }

        public void AddDataToLeft(EntityData data)
        {
            _leftChild = new SemanticTree(this, null, null, data);
        }


        public void AddDataToRight(EntityData data)
        {
            _rightChild = new SemanticTree(this, null, null, data);
        }

        public SemanticTree FindUp(SemanticTree startVertex, string lexemeImage)
        {
            SemanticTree tree = startVertex;
            while ((tree != null) && (lexemeImage != tree.Data?.LexemeImage))
            {
                tree = tree._parent;
            }
            return tree;
        }

        public SemanticTree FindUp(string lexemeImage) => FindUp(this, lexemeImage);

        public SemanticTree FindRightLeft(SemanticTree startVertex, string lexemeImage)
        {
            SemanticTree right = startVertex._rightChild;
            while ((right != null) && (lexemeImage != right.Data?.LexemeImage))
            {
                right = right._leftChild;
            }
            return right;
        }

        public SemanticTree FindRightLeft(string lexemeImage) => FindRightLeft(this, lexemeImage); 

        public SemanticTree FindVertexWithEqualLexemeOnOneLevel(SemanticTree vertexFrom, string lexemeImage)
        {
            SemanticTree tree = vertexFrom;
            while ((tree != null) && (tree._parent?._rightChild != tree))
            {
                if (tree.Data?.LexemeImage == lexemeImage)
                    return tree;
                tree = tree._parent;
            }
            return null;
        }
        protected SemanticTree() { }
        /// <summary>
        /// Копирует семантическое дерево, начиная с текущего узла - т.е. вызывающего объекта.
        /// </summary>
        /// <returns></returns>
        public SemanticTree Copy() => Copy(this);
        /// <summary>
        /// Полное копирование указанного дерева
        /// </summary>
        /// <param name="startNode">корень дерева, которое необходимо скопировать</param>
        /// <param name="desiredParent">Родитель, которого необходимо присвоить копии</param>
        /// <returns></returns>
        public SemanticTree Copy(SemanticTree startNode, SemanticTree desiredParent = null)
        {
            if (startNode == null)
                return null;
            SemanticTree result = new SemanticTree();
            result.Data = startNode.Data?.Clone();
            //result.IsInterpret = startNode.IsInterpret;
            result.CurrentVertex = startNode.CurrentVertex;
            result._parent = desiredParent;
            if (startNode._leftChild != null)
                result._leftChild = startNode.Copy(startNode._leftChild, result);
            if (startNode._rightChild != null)
                result._rightChild = startNode.Copy(startNode._rightChild, result);
            return result;
        }
        #endregion

        #region SemanticPrograms

        public SemanticTree IncludeLexeme(string lexemeImage, LexemeImageCategory category)
        {
            if (IsLexemeRepeatsInBlock(CurrentVertex, lexemeImage))
            {
                throw new Exception($"Лексема '{lexemeImage}' уже была описана ранее");
            }
            EntityData data;
            SemanticTree vertex;
            if (category == LexemeImageCategory.ClassType)
            {
                data = new EntityData() { LexemeImage = lexemeImage, Category = category, Lexeme = Lexemes.TypeClass, DataType = DataTypesTable.UserType};
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем класса
                CurrentVertex._leftChild = vertex;  // привязали его как левого потомка
                vertex._rightChild = new SemanticTree(vertex, null, null, null);  // сразу создали путого правого потомка
                CurrentVertex = vertex._rightChild; // текущий указатель "внутри класса"
                return vertex;  // вернули адрес возврата
            }
            else if (category == LexemeImageCategory.Function)
            {
                data = new EntityData() { LexemeImage = lexemeImage, Category = category, Lexeme = Lexemes.TypeIdentifier};
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем функции
                CurrentVertex._leftChild = vertex;  // привязали его как левого потомка
                vertex._rightChild = new SemanticTree(vertex, null, null, null);  // сразу создали путого правого потомка
                CurrentVertex = vertex._rightChild; // текущий указатель "внутри класса"
                return vertex;  // вернули адрес возврата
            }
            else
            {
                data = new EntityData() { LexemeImage = lexemeImage, Category = category, };
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем класса
                CurrentVertex._leftChild = vertex;  // привязали его как левого потомка
                CurrentVertex = vertex;
                return CurrentVertex;
            }
        }

        public bool IsLexemeRepeatsInBlock(SemanticTree blockVertex, string lexemeImage)
        {
            if (FindVertexWithEqualLexemeOnOneLevel(blockVertex, lexemeImage) == null)
                return false;
            return true;
        }

        public SemanticTree IncludeCompoundOperator()
        {
            // надо вернуть не vertex, а его родителя, в месте, где эта функция вызывается это фиксится
            SemanticTree vertex = new SemanticTree(CurrentVertex, null, null, null);
            vertex._rightChild = new SemanticTree(vertex, null, null, null);
            CurrentVertex._leftChild = vertex;
            CurrentVertex = vertex._rightChild;
            return vertex;
        }

        public SemanticTree IncludeConstant(string lexemeImage, int dataType, LexemeValue lexemeValue = null)
        {
            if (IsLexemeRepeatsInBlock(CurrentVertex, lexemeImage))
            {
                throw new Exception($"Константа '{lexemeImage}' уже была описана ранее");
            }
            EntityData data = new EntityData() { Category = LexemeImageCategory.Constant, IsConstant = true, DataType = dataType,
                Lexeme = Lexemes.TypeIdentifier, LexemeImage = lexemeImage, LexemeValue = lexemeValue };
            SemanticTree vertex = new SemanticTree(CurrentVertex, null, null, data);
            CurrentVertex._leftChild = vertex;
            CurrentVertex = vertex;
            return CurrentVertex;
        }

        public SemanticTree IncludeVariable(string lexemeImage, int dataType, LexemeValue lexemeValue = null)
        {
            if (IsLexemeRepeatsInBlock(CurrentVertex, lexemeImage))
            {
                throw new Exception($"Переменная '{lexemeImage}' уже была описана ранее");
            }
            EntityData data = new EntityData()
            {
                Category = LexemeImageCategory.Variable,
                IsConstant = false,
                DataType = dataType,
                Lexeme = Lexemes.TypeIdentifier,
                LexemeImage = lexemeImage,
                LexemeValue = lexemeValue
            };
            SemanticTree vertex = new SemanticTree(CurrentVertex, null, null, data);
            CurrentVertex._leftChild = vertex;
            CurrentVertex = vertex;
            return CurrentVertex;
        }
        /// <summary>
        /// Вставка объекта в семантическу таблицу
        /// </summary>
        /// <param name="variableImage">Вид объекта класса</param>
        /// <param name="classNameImage">Вид типа объекта класса</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public SemanticTree IncudeClassObject(string variableImage, string classNameImage)
        {
            SemanticTree classDescription = FindUp(CurrentVertex, classNameImage);
            if (classDescription == null)
                throw new Exception($"Класс {classNameImage} ни разу не описан");
            if (IsLexemeRepeatsInBlock(CurrentVertex, variableImage))
                throw new Exception($"Объект '{variableImage}' уже был описан ранее");
            EntityData data = new EntityData()
            {
                Category = LexemeImageCategory.ClassObject,
                IsConstant = false,
                DataType = DataTypesTable.UserType,
                Lexeme = Lexemes.TypeIdentifier,
                LexemeImage = variableImage,
                LexemeValue = null
            };
            SemanticTree vertex = new SemanticTree(CurrentVertex, null, null, data);
            CurrentVertex._leftChild = vertex;
            CurrentVertex = vertex;
            SemanticTree classDescriptionBody = classDescription.Copy(classDescription._rightChild, null);
            classDescriptionBody._parent = CurrentVertex;
            CurrentVertex._rightChild = classDescriptionBody;
            return CurrentVertex;
        }

        #endregion

        public void Print() => Print(0);

        /// <summary>
        /// Выводит структуру дерева на консоль. Только для отладки
        /// </summary>
        /// <param name="level"></param>
        protected void Print(int level)
        {
            for (int i = 0; i < level; i++)
            {
                Console.Write("  ");
            }
            if (Data != null)
                switch (Data.Category)
                {
                    case LexemeImageCategory.Constant:
                        {
                            Console.WriteLine($"Константа '{Data.LexemeImage}'");
                            break;
                        }
                    case LexemeImageCategory.Variable:
                        {
                            Console.WriteLine($"Переменная '{Data.LexemeImage}'");
                            break;
                        }
                    case LexemeImageCategory.Function:
                        {
                            Console.WriteLine($"Функция '{Data.LexemeImage}'");
                            break;
                        }
                    case LexemeImageCategory.ClassType:
                        {
                            Console.WriteLine($"Описание класса '{Data.LexemeImage}'");
                            break;
                        }
                    case LexemeImageCategory.ClassObject:
                        {
                            Console.WriteLine($"Объект '{Data.LexemeImage}'");
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Пустой узел");
                            break;
                        }
                }
            else
                Console.WriteLine("Пустой узел");
            if (_rightChild != null)
                _rightChild.Print(level + 1);
            if (_leftChild != null)
                _leftChild.Print(level);
        }

        public override string ToString()
        {
            if (Data == null)
                return "Пустой узел";
            else
                return Data.LexemeImage;
        }
    }
}

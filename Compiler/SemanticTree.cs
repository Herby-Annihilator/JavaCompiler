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
        public EntityData Data { get; set; }  // данные о сущности

        public bool IsInterpret { get; set; } = false; // флаг интерпретации

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

                data = new EntityData() { LexemeImage = lexemeImage, Category = category, Lexeme = Lexemes.TypeClass};
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем класса
                CurrentVertex._leftChild = vertex;  // привязали его как левого потомка
                vertex._rightChild = new SemanticTree(vertex, null, null, null);  // сразу создали путого правого потомка
                CurrentVertex = vertex._rightChild; // текущий указатель "внутри класса"
                return vertex;  // вернули адрес возврата
            }
            else if (category == LexemeImageCategory.Function)
            {
                data = new EntityData() { LexemeImage = lexemeImage, Category = category, Lexeme = Lexemes.TypeIdentifier};
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем класса
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

        #endregion

        /// <summary>
        /// Выводит структуру дерева на консоль. Только для отладки
        /// </summary>
        /// <param name="level"></param>
        public void Print(int level)
        {
            for (int i = 0; i < level; i++)
            {
                Console.Write("\t");
            }

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

            if (_rightChild != null)
                _rightChild.Print(level + 1);
            if (_leftChild != null)
                _leftChild.Print(level);
        }
    }
}

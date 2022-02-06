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
        private SemanticTree _rightChild;
        private Node _data;
        public SemanticTree CurrentVertex { get; set; }


        #region TreeMethods
        public SemanticTree(SemanticTree parent, SemanticTree leftChild, SemanticTree rightChild, Node data)
        {
            _parent = parent;
            _leftChild = leftChild;
            _rightChild = rightChild;
            _data = data;
        }

        public void AddDataToLeft(Node data)
        {
            _leftChild = new SemanticTree(this, null, null, data);
        }


        public void AddDataToRight(Node data)
        {
            _rightChild = new SemanticTree(this, null, null, data);
        }

        public SemanticTree FindUp(SemanticTree startVertex, Lexemes lexeme)
        {
            SemanticTree tree = startVertex;
            while ((tree != null) && (lexeme != _data.Lexeme))
            {
                tree = tree._parent;
            }
            return tree;
        }

        public SemanticTree FindUp(Lexemes lexeme) => FindUp(this, lexeme);

        public SemanticTree FindRightLeft(SemanticTree startVertex, Lexemes lexeme)
        {
            SemanticTree right = startVertex._rightChild;
            while ((right != null) && (lexeme != right._data.Lexeme))
            {
                right = right._leftChild;
            }
            return right;
        }

        public SemanticTree FindRightLeft(Lexemes lexeme) => FindRightLeft(this, lexeme); 

        public SemanticTree FindVertexWithEqualLexemeOnOneLevel(SemanticTree vertexFrom, string lexemeImage)
        {
            SemanticTree tree = vertexFrom;
            while ((tree != null) && (tree._parent?._rightChild != tree))
            {
                if (tree._data.LexemeImage == lexemeImage)
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
            Node data;
            SemanticTree vertex;
            if (category == LexemeImageCategory.ClassType)
            {
                data = new Node() { LexemeImage = lexemeImage, Category = category, Lexeme = Lexemes.TypeClass};
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем класса
                CurrentVertex._leftChild = vertex;  // привязали его как левого потомка
                vertex._rightChild = new SemanticTree(vertex, null, null, null);  // сразу создали путого правого потомка
                CurrentVertex = vertex._rightChild; // текущий указатель "внутри класса"
                return vertex;  // вернули адрес возврата. Предусмотреть стэк в синтаксическом анализаторе
            }
            else if (category == LexemeImageCategory.Function)
            {
                data = new Node() { LexemeImage = lexemeImage, Category = category, Lexeme = Lexemes.TypeIdentifier};
                vertex = new SemanticTree(CurrentVertex, null, null, data);  // создали левого потомка с именем класса
                CurrentVertex._leftChild = vertex;  // привязали его как левого потомка
                vertex._rightChild = new SemanticTree(vertex, null, null, null);  // сразу создали путого правого потомка
                CurrentVertex = vertex._rightChild; // текущий указатель "внутри класса"
                return vertex;  // вернули адрес возврата. Предусмотреть стэк в синтаксическом анализаторе
            }
            else
            {
                data = new Node() { LexemeImage = lexemeImage, Category = category, };
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

        #endregion
    }
}

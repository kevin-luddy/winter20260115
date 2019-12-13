// no version info, downloaded 7/12/2018
(function e(t, n, r) { function s(o, u) { if (!n[o]) { if (!t[o]) { var a = typeof require == "function" && require; if (!u && a) return a(o, !0); if (i) return i(o, !0); throw new Error("Cannot find module '" + o + "'") } var f = n[o] = { exports: {} }; t[o][0].call(f.exports, function (e) { var n = t[o][1][e]; return s(n ? n : e) }, f, f.exports, e, t, n, r) } return n[o].exports } var i = typeof require == "function" && require; for (var o = 0; o < r.length; o++) s(r[o]); return s })({
    1: [function (require, module, exports) {
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _search = require('./search');

Object.keys(_search).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _search[key];
    }
  });
});

},{"./search":2}],2:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.searchFactory = searchFactory;

var _visitor = require('../../traversal/visitor');

searchFactory.$inject = ['uitsTraverseBuilder', 'uitsMatcherFactory'];
function searchFactory(traverseBuilder, matcherFactory) {

  var matchParentVisitor = new _visitor.MatchParentVisitor();
  var matchChildrenVisitor = new _visitor.MatchChildrenVisitor();

  return function (nodes, query) {
    var matcher = matcherFactory(query);
    var visitors = [new _visitor.MatchVisitor(matcher), matchChildrenVisitor, matchParentVisitor];

    var traverser = traverseBuilder.setVisitors(visitors).get();

    return (nodes || []).filter(function (node) {
      return traverser.traverse(node);
    });
  };
}

},{"../../traversal/visitor":12}],3:[function(require,module,exports){
arguments[4][1][0].apply(exports,arguments)
},{"./search":4}],4:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.searchFilter = searchFilter;
searchFilter.$inject = ['uitsSearch'];
function searchFilter(search) {
  return function (nodes, query) {
    return search(nodes, query);
  };
}

},{}],5:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _matcherFactory = require('./matcher-factory');

Object.keys(_matcherFactory).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _matcherFactory[key];
    }
  });
});

var _traverseBuilder = require('./traverse-builder');

Object.keys(_traverseBuilder).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _traverseBuilder[key];
    }
  });
});

},{"./matcher-factory":6,"./traverse-builder":7}],6:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.matcherFactoryProvider = matcherFactoryProvider;

var _matcherFactory = require('../../matcher-factory/matcher-factory');

function matcherFactoryProvider() {

  var _properties = ['title'];
  var _match = function _match(node, property, query) {
    return (node[property] || '').indexOf(query) > -1;
  };

  this.setProperties = function (properties) {
    return _properties = properties;
  };

  this.$get = function () {
    return (0, _matcherFactory.matcherFactory)(_properties, _match);
  };
}

},{"../../matcher-factory/matcher-factory":9}],7:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.traverseBuilderProvider = traverseBuilderProvider;

var _traverserBuilder = require('../../traversal/traverser-builder');

function traverseBuilderProvider() {

  var _childNodePath = 'nodes';
  this.setChildNodePath = function (childNodePath) {
    return _childNodePath = childNodePath;
  };

  this.$get = function () {
    return new _traverserBuilder.TraverserBuilder(_childNodePath);
  };
}

},{"../../traversal/traverser-builder":10}],8:[function(require,module,exports){
(function (global){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _angular = (typeof window !== "undefined" ? window['angular'] : typeof global !== "undefined" ? global['angular'] : null);

var angular = _interopRequireWildcard(_angular);

var _providers = require('./angular/providers');

var _factory = require('./angular/factory');

var _filters = require('./angular/filters');

function _interopRequireWildcard(obj) { if (obj && obj.__esModule) { return obj; } else { var newObj = {}; if (obj != null) { for (var key in obj) { if (Object.prototype.hasOwnProperty.call(obj, key)) newObj[key] = obj[key]; } } newObj.default = obj; return newObj; } }

exports.default = angular.module('ui.tree-search', []).provider('uitsMatcherFactory', _providers.matcherFactoryProvider).provider('uitsTraverseBuilder', _providers.traverseBuilderProvider).factory('uitsSearch', _factory.searchFactory).filter('uitsSearch', _filters.searchFilter);

}).call(this,typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})
},{"./angular/factory":1,"./angular/filters":3,"./angular/providers":5}],9:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.matcherFactory = matcherFactory;
function matcherFactory(properties, match) {
  return function (query) {
    return query == null ? function (node) {
      return true;
    } : function (node) {
      return properties.reduce(function (matched, property) {
        return matched || match(node, property, query);
      }, false);
    };
  };
}

},{}],10:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.TraverserBuilder = undefined;

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _traverser = require("./traverser");

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var TraverserBuilder = exports.TraverserBuilder = function () {
  function TraverserBuilder(childNodePath) {
    _classCallCheck(this, TraverserBuilder);

    this.childNodePath = childNodePath;
    this.visitors = [];
  }

  _createClass(TraverserBuilder, [{
    key: "addVisitors",
    value: function addVisitors(visitor) {
      this.visitors.push(visitor);

      return this;
    }
  }, {
    key: "setVisitors",
    value: function setVisitors(visitors) {
      this.visitors = visitors;

      return this;
    }
  }, {
    key: "get",
    value: function get() {
      var _this = this;

      return new _traverser.Traverser(function (node) {
        return node[_this.childNodePath] || [];
      }, this.visitors);
    }
  }]);

  return TraverserBuilder;
}();

},{"./traverser":11}],11:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var Traverser = exports.Traverser = function () {
  function Traverser(children) {
    var visitors = arguments.length <= 1 || arguments[1] === undefined ? [] : arguments[1];

    _classCallCheck(this, Traverser);

    this.children = children;
    this.visitors = visitors;
  }

  _createClass(Traverser, [{
    key: "traverse",
    value: function traverse(node) {
      var _this = this;

      return this.visitors.reduce(function (node, visitor) {
        return _this.traverseForVisitor(node, visitor);
      }, node);
    }
  }, {
    key: "traverseForVisitor",
    value: function traverseForVisitor(node, visitor) {
      var _this2 = this;

      node = visitor.enterNode(node);

      this.children(node).forEach(function (child) {
        _this2.traverseForVisitor(child, visitor);
      });

      return visitor.leaveNode(node);
    }
  }]);

  return Traverser;
}();

},{}],12:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _visitor = require('./visitor');

Object.keys(_visitor).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _visitor[key];
    }
  });
});

var _matchVisitor = require('./match-visitor');

Object.keys(_matchVisitor).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _matchVisitor[key];
    }
  });
});

var _matchParentVisitor = require('./match-parent-visitor');

Object.keys(_matchParentVisitor).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _matchParentVisitor[key];
    }
  });
});

var _matchChildrenVisitor = require('./match-children-visitor');

Object.keys(_matchChildrenVisitor).forEach(function (key) {
  if (key === "default") return;
  Object.defineProperty(exports, key, {
    enumerable: true,
    get: function get() {
      return _matchChildrenVisitor[key];
    }
  });
});

},{"./match-children-visitor":13,"./match-parent-visitor":14,"./match-visitor":15,"./visitor":16}],13:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.MatchChildrenVisitor = undefined;

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _get = function get(object, property, receiver) { if (object === null) object = Function.prototype; var desc = Object.getOwnPropertyDescriptor(object, property); if (desc === undefined) { var parent = Object.getPrototypeOf(object); if (parent === null) { return undefined; } else { return get(parent, property, receiver); } } else if ("value" in desc) { return desc.value; } else { var getter = desc.get; if (getter === undefined) { return undefined; } return getter.call(receiver); } };

var _visitor = require('./visitor');

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var MatchChildrenVisitor = exports.MatchChildrenVisitor = function (_Visitor) {
  _inherits(MatchChildrenVisitor, _Visitor);

  function MatchChildrenVisitor() {
    _classCallCheck(this, MatchChildrenVisitor);

    var _this = _possibleConstructorReturn(this, Object.getPrototypeOf(MatchChildrenVisitor).call(this));

    _this.matchedNode = null;
    return _this;
  }

  _createClass(MatchChildrenVisitor, [{
    key: 'enterNode',
    value: function enterNode(node) {
      node = _get(Object.getPrototypeOf(MatchChildrenVisitor.prototype), 'enterNode', this).call(this, node);

      if (this.matchedNode) {
        node.$matched = true;
      } else if (node.$matched) {
        this.matchedNode = node;
      }

      return node;
    }
  }, {
    key: 'leaveNode',
    value: function leaveNode(node) {
      node = _get(Object.getPrototypeOf(MatchChildrenVisitor.prototype), 'leaveNode', this).call(this, node);

      if (this.matchedNode === node) {
        this.matchedNode = null;
      }

      return node;
    }
  }]);

  return MatchChildrenVisitor;
}(_visitor.Visitor);

},{"./visitor":16}],14:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.MatchParentVisitor = undefined;

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _get = function get(object, property, receiver) { if (object === null) object = Function.prototype; var desc = Object.getOwnPropertyDescriptor(object, property); if (desc === undefined) { var parent = Object.getPrototypeOf(object); if (parent === null) { return undefined; } else { return get(parent, property, receiver); } } else if ("value" in desc) { return desc.value; } else { var getter = desc.get; if (getter === undefined) { return undefined; } return getter.call(receiver); } };

var _visitor = require('./visitor');

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var MatchParentVisitor = exports.MatchParentVisitor = function (_Visitor) {
  _inherits(MatchParentVisitor, _Visitor);

  function MatchParentVisitor() {
    _classCallCheck(this, MatchParentVisitor);

    return _possibleConstructorReturn(this, Object.getPrototypeOf(MatchParentVisitor).apply(this, arguments));
  }

  _createClass(MatchParentVisitor, [{
    key: 'leaveNode',
    value: function leaveNode(node) {
      node = _get(Object.getPrototypeOf(MatchParentVisitor.prototype), 'leaveNode', this).call(this, node);

      if (node.$matched) {
        this.stack.forEach(function (node) {
          return node.$matched = true;
        });
      }

      return node;
    }
  }]);

  return MatchParentVisitor;
}(_visitor.Visitor);

},{"./visitor":16}],15:[function(require,module,exports){
'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.MatchVisitor = undefined;

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _get = function get(object, property, receiver) { if (object === null) object = Function.prototype; var desc = Object.getOwnPropertyDescriptor(object, property); if (desc === undefined) { var parent = Object.getPrototypeOf(object); if (parent === null) { return undefined; } else { return get(parent, property, receiver); } } else if ("value" in desc) { return desc.value; } else { var getter = desc.get; if (getter === undefined) { return undefined; } return getter.call(receiver); } };

var _visitor = require('./visitor');

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var MatchVisitor = exports.MatchVisitor = function (_Visitor) {
  _inherits(MatchVisitor, _Visitor);

  function MatchVisitor(match) {
    _classCallCheck(this, MatchVisitor);

    var _this = _possibleConstructorReturn(this, Object.getPrototypeOf(MatchVisitor).call(this));

    _this.match = match;
    return _this;
  }

  _createClass(MatchVisitor, [{
    key: 'enterNode',
    value: function enterNode(node) {
      node = _get(Object.getPrototypeOf(MatchVisitor.prototype), 'enterNode', this).call(this, node);

      node.$matched = this.match(node);

      return node;
    }
  }]);

  return MatchVisitor;
}(_visitor.Visitor);

},{"./visitor":16}],16:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var Visitor = exports.Visitor = function () {
  function Visitor() {
    _classCallCheck(this, Visitor);

    this.stack = [];
  }

  _createClass(Visitor, [{
    key: "enterNode",
    value: function enterNode(node) {
      this.stack.push(node);

      return node;
    }
  }, {
    key: "leaveNode",
    value: function leaveNode(node) {
      this.stack.pop();

      return node;
    }
  }]);

  return Visitor;
}();

},{}]},{},[8])
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiIiwic291cmNlcyI6WyJpbmRleC5qcyJdLCJzb3VyY2VzQ29udGVudCI6WyJpbXBvcnQgKiBhcyBhbmd1bGFyIGZyb20gJ2FuZ3VsYXInO1xuaW1wb3J0IHsgbWF0Y2hlckZhY3RvcnlQcm92aWRlciwgdHJhdmVyc2VCdWlsZGVyUHJvdmlkZXIgfSBmcm9tICcuL2FuZ3VsYXIvcHJvdmlkZXJzJztcbmltcG9ydCB7IHNlYXJjaEZhY3RvcnkgfSBmcm9tICcuL2FuZ3VsYXIvZmFjdG9yeSc7XG5pbXBvcnQgeyBzZWFyY2hGaWx0ZXIgfSBmcm9tICcuL2FuZ3VsYXIvZmlsdGVycyc7XG5cbmV4cG9ydCBkZWZhdWx0IGFuZ3VsYXJcbiAgLm1vZHVsZSgndWkudHJlZS1zZWFyY2gnLCBbXSlcbiAgLnByb3ZpZGVyKCd1aXRzTWF0Y2hlckZhY3RvcnknLCBtYXRjaGVyRmFjdG9yeVByb3ZpZGVyKVxuICAucHJvdmlkZXIoJ3VpdHNUcmF2ZXJzZUJ1aWxkZXInLCB0cmF2ZXJzZUJ1aWxkZXJQcm92aWRlcilcbiAgLmZhY3RvcnkoJ3VpdHNTZWFyY2gnLCBzZWFyY2hGYWN0b3J5KVxuICAuZmlsdGVyKCd1aXRzU2VhcmNoJywgc2VhcmNoRmlsdGVyKVxuO1xuIl0sImZpbGUiOiJhbmd1bGFyLXVpLXRyZWUtc2VhcmNoLmpzIiwic291cmNlUm9vdCI6Ii9zb3VyY2UvIn0=

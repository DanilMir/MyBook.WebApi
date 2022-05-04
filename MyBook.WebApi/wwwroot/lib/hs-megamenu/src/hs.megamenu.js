;
(function($) {
    'use strict';
    var MegaMenu = window.MegaMenu || {};
    MegaMenu = (function() {
        function MegaMenu(element, options) {
            var self = this;
            this.$element = $(element);
            this.options = $.extend(true, {}, MegaMenu.defaults, options);
            this._items = $();
            Object.defineProperties(this, {
                itemsSelector: {
                    get: function() {
                        return self.options.classMap.hasSubMenu + ',' +
                            self.options.classMap.hasMegaMenu;
                    }
                },
                _tempChain: {
                    value: null,
                    writable: true
                },
                state: {
                    value: null,
                    writable: true
                }
            });
            this.initialize();
        }
        return MegaMenu;
    }());
    MegaMenu.defaults = {
        event: 'hover',
        direction: 'horizontal',
        breakpoint: 991,
        animationIn: false,
        animationOut: false,
        rtl: false,
        hideTimeOut: 300,
        sideBarRatio: 1 / 4,
        pageContainer: $('body'),
        classMap: {
            initialized: '.hs-menu-initialized',
            mobileState: '.hs-mobile-state',
            subMenu: '.hs-sub-menu',
            hasSubMenu: '.hs-has-sub-menu',
            hasSubMenuActive: '.hs-sub-menu-opened',
            megaMenu: '.hs-mega-menu',
            hasMegaMenu: '.hs-has-mega-menu',
            hasMegaMenuActive: '.hs-mega-menu-opened'
        },
        mobileSpeed: 400,
        mobileEasing: 'linear',
        isMenuOpened: false,
        beforeOpen: function() {},
        beforeClose: function() {},
        afterOpen: function() {},
        afterClose: function() {}
    };
    MegaMenu.prototype.initialize = function() {
        var self = this,
            $w = $(window);
        if (this.options.rtl) this.$element.addClass('hs-rtl');
        this.$element.addClass(this.options.classMap.initialized.slice(1)).addClass('hs-menu-' + this.options.direction);
        $w.on('resize.HSMegaMenu', function(e) {
            if (self.resizeTimeOutId) clearTimeout(self.resizeTimeOutId);
            self.resizeTimeOutId = setTimeout(function() {
                if (window.innerWidth <= self.options.breakpoint && self.state === 'desktop') self.initMobileBehavior();
                else if (window.innerWidth > self.options.breakpoint && self.state === 'mobile') self.initDesktopBehavior();
                self.refresh();
            }, 50);
        });
        if (window.innerWidth >= 768) {
            $(document).on('click.HSMegaMenu touchstart.HSMegaMenu', 'body', function(e) {
                var $parents = $(e.target).parents(self.itemsSelector);
                self.closeAll($parents.add($(e.target)));
            });
        }
        $w.on('keyup.HSMegaMenu', function(e) {
            if (e.keyCode && e.keyCode === 27) {
                self.closeAll();
                self.options.isMenuOpened = false;
            }
        });
        if (window.innerWidth <= this.options.breakpoint) this.initMobileBehavior();
        else if (window.innerWidth > this.options.breakpoint) this.initDesktopBehavior();
        this.smartPositions();
        return this;
    };
    MegaMenu.prototype.smartPositions = function() {
        var self = this,
            $submenus = this.$element.find(this.options.classMap.subMenu);
        $submenus.each(function(i, el) {
            MenuItem.smartPosition($(el), self.options);
        });
    };
    MegaMenu.prototype.bindEvents = function() {
        var self = this,
            selector;
        if (this.options.event === 'hover' && !_isTouch()) {
            this.$element.on('mouseenter.HSMegaMenu', this.options.classMap.hasMegaMenu + ':not([data-event="click"]),' +
                this.options.classMap.hasSubMenu + ':not([data-event="click"])',
                function(e) {
                    var $this = $(this),
                        $chain = $this.parents(self.itemsSelector);
                    if (!$this.data('HSMenuItem')) {
                        self.initMenuItem($this, self.getType($this));
                    }
                    $chain = $chain.add($this);
                    self.closeAll($chain);
                    $chain.each(function(i, el) {
                        var HSMenuItem = $(el).data('HSMenuItem');
                        if (HSMenuItem.hideTimeOutId) clearTimeout(HSMenuItem.hideTimeOutId);
                        HSMenuItem.desktopShow();
                    });
                    self._items = self._items.not($chain);
                    self._tempChain = $chain;
                    e.preventDefault();
                    e.stopPropagation();
                }).on('mouseleave.HSMegaMenu', this.options.classMap.hasMegaMenu + ':not([data-event="click"]),' +
                this.options.classMap.hasSubMenu + ':not([data-event="click"])',
                function(e) {
                    if (!$(this).data('HSMenuItem')) return;
                    var $this = $(this),
                        HSMenuItem = $this.data('HSMenuItem'),
                        $chain = $(e.relatedTarget).parents(self.itemsSelector);
                    HSMenuItem.hideTimeOutId = setTimeout(function() {
                        self.closeAll($chain);
                    }, self.options.hideTimeOut);
                    self._items = self._items.add(self._tempChain);
                    self._tempChain = null;
                    e.preventDefault();
                    e.stopPropagation();
                }).on('click.HSMegaMenu touchstart.HSMegaMenu', this.options.classMap.hasMegaMenu + '[data-event="click"] > a, ' +
                this.options.classMap.hasSubMenu + '[data-event="click"] > a',
                function(e) {
                    var $this = $(this).parent('[data-event="click"]'),
                        HSMenuItem;
                    if (!$this.data('HSMenuItem')) {
                        self.initMenuItem($this, self.getType($this));
                    }
                    self.closeAll($this.add($this.parents(self.itemsSelector)));
                    HSMenuItem = $this.data('HSMenuItem');
                    if (HSMenuItem.isOpened) {
                        HSMenuItem.desktopHide();
                    } else {
                        HSMenuItem.desktopShow();
                    }
                    e.preventDefault();
                    e.stopPropagation();
                });
        } else {
            this.$element.on('click.HSMegaMenu', (_isTouch() ? this.options.classMap.hasMegaMenu + ' > a, ' +
                this.options.classMap.hasSubMenu + ' > a' : this.options.classMap.hasMegaMenu + ':not([data-event="hover"]) > a,' +
                this.options.classMap.hasSubMenu + ':not([data-event="hover"]) > a'), function(e) {
                var $this = $(this).parent(),
                    HSMenuItem, $parents = $this.parents(self.itemsSelector);
                if (!$this.data('HSMenuItem')) {
                    self.initMenuItem($this, self.getType($this));
                }
                self.closeAll($this.add($this.parents(self.itemsSelector)));
                HSMenuItem = $this.addClass('hs-event-prevented').data('HSMenuItem');
                if (HSMenuItem.isOpened) {
                    HSMenuItem.desktopHide();
                } else {
                    HSMenuItem.desktopShow();
                }
                e.preventDefault();
                e.stopPropagation();
            });
            if (!_isTouch()) {
                this.$element.on('mouseenter.HSMegaMenu', this.options.classMap.hasMegaMenu + '[data-event="hover"],' +
                    this.options.classMap.hasSubMenu + '[data-event="hover"]',
                    function(e) {
                        var $this = $(this),
                            $parents = $this.parents(self.itemsSelector);
                        if (!$this.data('HSMenuItem')) {
                            self.initMenuItem($this, self.getType($this));
                        }
                        self.closeAll($this.add($parents));
                        $parents.add($this).each(function(i, el) {
                            var HSMenuItem = $(el).data('HSMenuItem');
                            if (HSMenuItem.hideTimeOutId) clearTimeout(HSMenuItem.hideTimeOutId);
                            HSMenuItem.desktopShow();
                        });
                        e.preventDefault();
                        e.stopPropagation();
                    }).on('mouseleave.HSMegaMenu', this.options.classMap.hasMegaMenu + '[data-event="hover"],' +
                    this.options.classMap.hasSubMenu + '[data-event="hover"]',
                    function(e) {
                        var $this = $(this),
                            HSMenuItem = $this.data('HSMenuItem');
                        HSMenuItem.hideTimeOutId = setTimeout(function() {
                            self.closeAll($(e.relatedTarget).parents(self.itemsSelector));
                        }, self.options.hideTimeOut);
                        e.preventDefault();
                        e.stopPropagation();
                    })
            }
        }
        this.$element.on('keydown.HSMegaMenu', this.options.classMap.hasMegaMenu + ' > a,' +
            this.options.classMap.hasSubMenu + ' > a',
            function(e) {
                var $this = $(this),
                    $parent = $this.parent(),
                    HSMenuItem;
                if (!$parent.data('HSMenuItem')) {
                    self.initMenuItem($parent, self.getType($parent));
                }
                HSMenuItem = $parent.data('HSMenuItem');
                if ($this.is(':focus')) {
                    if (e.keyCode && e.keyCode === 40) {
                        e.preventDefault();
                        HSMenuItem.desktopShow();
                        self.options.isMenuOpened = true;
                    }
                    if (e.keyCode && e.keyCode === 13) {
                        if (self.options.isMenuOpened === true) {
                            HSMenuItem.desktopHide();
                            self.options.isMenuOpened = false;
                        } else {
                            HSMenuItem.desktopShow();
                            self.options.isMenuOpened = true;
                        }
                    }
                }
                $this.on('focusout', function() {
                    self.options.isMenuOpened = false;
                });
                HSMenuItem.menu.find('a').on('focusout', function() {
                    setTimeout(function() {
                        if (!HSMenuItem.menu.find('a').is(':focus')) {
                            HSMenuItem.desktopHide();
                            self.options.isMenuOpened = false;
                        }
                    });
                });
            })
    };
    MegaMenu.prototype.initMenuItem = function(element, type) {
        var self = this,
            Item = new MenuItem(element, element.children(self.options.classMap[type === 'mega-menu' ? 'megaMenu' : 'subMenu']), $.extend(true, {
                type: type
            }, self.options, element.data()), self.$element);
        element.data('HSMenuItem', Item);
        this._items = this._items.add(element);
    };
    MegaMenu.prototype.initMobileBehavior = function() {
        var self = this;
        this.state = 'mobile';
        this.$element.off('.HSMegaMenu').addClass(this.options.classMap.mobileState.slice(1)).on('click.HSMegaMenu', self.options.classMap.hasSubMenu + ' > a, ' + self.options.classMap.hasMegaMenu + ' > a', function(e) {
            var $this = $(this).parent(),
                MenuItemInstance;
            if (!$this.data('HSMenuItem')) {
                self.initMenuItem($this, self.getType($this));
            }
            self.closeAll($this.parents(self.itemsSelector).add($this));
            MenuItemInstance = $this.data('HSMenuItem');
            if (MenuItemInstance.isOpened) {
                MenuItemInstance.mobileHide();
            } else {
                MenuItemInstance.mobileShow();
            }
            e.preventDefault();
            e.stopPropagation();
        }).find(this.itemsSelector).not(this.options.classMap.hasSubMenuActive + ',' +
            this.options.classMap.hasMegaMenuActive).children(this.options.classMap.subMenu + ',' +
            this.options.classMap.megaMenu).hide();
    };
    MegaMenu.prototype.initDesktopBehavior = function() {
        this.state = 'desktop';
        this.$element.removeClass(this.options.classMap.mobileState.slice(1)).off('.HSMegaMenu').find(this.itemsSelector).not(this.options.classMap.hasSubMenuActive + ',' +
            this.options.classMap.hasMegaMenuActive).children(this.options.classMap.subMenu + ',' +
            this.options.classMap.megaMenu).hide();
        this.bindEvents();
    };
    MegaMenu.prototype.closeAll = function(except) {
        var self = this;
        return this._items.not(except && except.length ? except : $()).each(function(i, el) {
            $(el).removeClass('hs-event-prevented').data('HSMenuItem')[self.state === 'mobile' ? 'mobileHide' : 'desktopHide']();
        });
    };
    MegaMenu.prototype.getType = function(item) {
        if (!item || !item.length) return null;
        return item.hasClass(this.options.classMap.hasSubMenu.slice(1)) ? 'sub-menu' : (item.hasClass(this.options.classMap.hasMegaMenu.slice(1)) ? 'mega-menu' : null);
    };
    MegaMenu.prototype.getState = function() {
        return this.state;
    };
    MegaMenu.prototype.refresh = function() {
        return this._items.add(this._tempChain).each(function(i, el) {
            $(el).data('HSMenuItem')._updateMenuBounds();
        });
    };

    function MenuItem(element, menu, options, container) {
        var self = this;
        this.$element = element;
        this.menu = menu;
        this.options = options;
        this.$container = container;
        Object.defineProperties(this, {
            itemClass: {
                get: function() {
                    return self.options.type === 'mega-menu' ? self.options.classMap.hasMegaMenu : self.options.classMap.hasSubMenu;
                }
            },
            activeItemClass: {
                get: function() {
                    return self.options.type === 'mega-menu' ? self.options.classMap.hasMegaMenuActive : self.options.classMap.hasSubMenuActive;
                }
            },
            menuClass: {
                get: function() {
                    return self.options.type === 'mega-menu' ? self.options.classMap.megaMenu : self.options.classMap.subMenu;
                }
            },
            isOpened: {
                get: function() {
                    return this.$element.hasClass(this.activeItemClass.slice(1));
                }
            }
        });
        this.menu.addClass('animated').on('click.HSMegaMenu', function(e) {
            self._updateMenuBounds();
        });
        if (this.$element.data('max-width')) this.menu.css('max-width', this.$element.data('max-width'));
        if (this.$element.data('position')) this.menu.addClass('hs-position-' + this.$element.data('position'));
        if (this.options.animationOut) {
            this.menu.on('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function(e) {
                if (self.menu.hasClass(self.options.animationOut)) {
                    self.$element.removeClass(self.activeItemClass.slice(1));
                    self.options.afterClose.call(self, self.$element, self.menu);
                }
                if (self.menu.hasClass(self.options.animationIn)) {
                    self.options.afterOpen.call(self, self.$element, self.menu);
                }
                e.stopPropagation();
                e.preventDefault();
            });
        }
    }
    MenuItem.prototype.desktopShow = function() {
        if (!this.menu.length) return this;
        this.$element.addClass(this.activeItemClass.slice(1));
        this._updateMenuBounds();
        this.menu.show();
        if (this.options.direction === 'horizontal') this.smartPosition(this.menu, this.options);
        if (this.options.animationOut) {
            this.menu.removeClass(this.options.animationOut);
        } else {
            this.options.afterOpen.call(this, this.$element, this.menu);
        }
        if (this.options.animationIn) {
            this.menu.addClass(this.options.animationIn)
        }
        return this;
    }
    MenuItem.prototype.desktopHide = function() {
        var self = this;
        if (!this.menu.length) return this;
        this.$element.removeClass(this.activeItemClass.slice(1));
        this.menu.hide();
        if (this.options.animationIn) {
            this.menu.removeClass(this.options.animationIn);
        }
        if (this.options.animationOut) {
            this.menu.addClass(this.options.animationOut);
        } else {
            this.options.afterClose.call(this, this.$element, this.menu);
        }
        return this;
    }
    MenuItem.prototype.mobileShow = function() {
        var self = this;
        if (!this.menu.length) return this;
        this.menu.removeClass(this.options.animationIn).removeClass(this.options.animationOut).stop().slideDown({
            duration: self.options.mobileSpeed,
            easing: self.options.mobileEasing,
            complete: function() {
                self.options.afterOpen.call(self, self.$element, self.menu);
            }
        });
        this.$element.addClass(this.activeItemClass.slice(1));
        return this;
    };
    MenuItem.prototype.mobileHide = function() {
        var self = this;
        if (!this.menu.length) return this;
        this.menu.stop().slideUp({
            duration: self.options.mobileSpeed,
            easing: self.options.mobileEasing,
            complete: function() {
                self.options.afterClose.call(self, self.$element, self.menu);
            }
        });
        this.$element.removeClass(this.activeItemClass.slice(1));
        return this;
    };
    MenuItem.prototype.smartPosition = function(element, options) {
        MenuItem.smartPosition(element, options);
    };
    MenuItem.smartPosition = function(element, options) {
        if (!element && !element.length) return;
        var $w = $(window);
        element.removeClass('hs-reversed');
        if (!options.rtl) {
            if (element.offset().left + element.outerWidth() > window.innerWidth) {
                element.addClass('hs-reversed');
            }
        } else {
            if (element.offset().left < 0) {
                element.addClass('hs-reversed');
            }
        }
    };
    MenuItem.prototype._updateMenuBounds = function() {
        var width = 'auto';
        if (this.options.direction === 'vertical' && this.options.type === 'mega-menu') {
            if (this.$container && this.$container.data('HSMegaMenu').getState() === 'desktop') {
                if (!this.options.pageContainer.length) this.options.pageContainer = $('body');
                width = this.options.pageContainer.outerWidth() * (1 - this.options.sideBarRatio);
            } else {
                width = 'auto';
            }
            this.menu.css({
                'width': width,
                'height': 'auto'
            });
            if (this.menu.outerHeight() > this.$container.outerHeight()) {
                return;
            }
            this.menu.css('height', '100%');
        }
    };
    $.fn.HSMegaMenu = function() {
        var _ = this,
            opt = arguments[0],
            args = Array.prototype.slice.call(arguments, 1),
            l = _.length,
            i, ret;
        for (i = 0; i < l; i++) {
            if (typeof opt === 'object' || typeof opt === 'undefined')
                _[i].MegaMenu = new MegaMenu(_[i], opt);
            else
                ret = _[i].MegaMenu[opt].apply(_[i].MegaMenu, args);
            if (typeof ret != 'undefined') return ret;
        }
        return _;
    };

    function _isTouch() {
        return ('ontouchstart' in window);
    }
})(jQuery);